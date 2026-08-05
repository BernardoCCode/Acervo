using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.DTOs.Search;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

public sealed class AcademicSearchService : IAcademicSearchPort
{
    private readonly OpenAlexSearchClient _openAlex;
    private readonly ArxivSearchClient _arxiv;
    private readonly CrossrefSearchClient _crossref;
    private readonly EuropePmcSearchClient _europePmc;
    private readonly SemanticScholarSearchClient _semanticScholar;
    private readonly AcademicSearchOptions _options;
    private readonly ILogger<AcademicSearchService> _logger;

    public AcademicSearchService(
        OpenAlexSearchClient openAlex,
        ArxivSearchClient arxiv,
        CrossrefSearchClient crossref,
        EuropePmcSearchClient europePmc,
        SemanticScholarSearchClient semanticScholar,
        IOptions<AcademicSearchOptions> options,
        ILogger<AcademicSearchService> logger)
    {
        _openAlex = openAlex;
        _arxiv = arxiv;
        _crossref = crossref;
        _europePmc = europePmc;
        _semanticScholar = semanticScholar;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        SearchFiltersRequest? filters,
        CancellationToken cancellationToken = default)
    {
        var enabledSources = filters?.Sources?.ToHashSet()
            ??
            [
                SourceSystem.OpenAlex,
                SourceSystem.PubMed,
                SourceSystem.Scholar,
                SourceSystem.ArXiv
            ];

        var tasks = new List<Task<IReadOnlyList<AcademicArticleCandidate>>>();

        if (_options.EnableOpenAlex && enabledSources.Contains(SourceSystem.OpenAlex))
            tasks.Add(SafeSearch(() => _openAlex.SearchAsync(query, cancellationToken), "OpenAlex"));

        if (_options.EnableEuropePmc && enabledSources.Contains(SourceSystem.PubMed))
            tasks.Add(SafeSearch(() => _europePmc.SearchAsync(query, cancellationToken), "EuropePMC"));

        if (_options.EnableSemanticScholar && enabledSources.Contains(SourceSystem.Scholar))
            tasks.Add(SafeSearch(() => _semanticScholar.SearchAsync(query, cancellationToken), "SemanticScholar"));

        if (_options.EnableArxiv && enabledSources.Contains(SourceSystem.ArXiv))
            tasks.Add(SafeSearch(() => _arxiv.SearchAsync(query, cancellationToken), "arXiv"));

        if (_options.EnableCrossref && enabledSources.Contains(SourceSystem.Crossref))
            tasks.Add(SafeSearch(() => _crossref.SearchAsync(query, cancellationToken), "Crossref"));

        var results = await Task.WhenAll(tasks);

        IEnumerable<AcademicArticleCandidate> ApplyFilters(
            IEnumerable<AcademicArticleCandidate> items)
        {
            if (filters?.YearMin is int yearMin)
                items = items.Where(a => a.Year is null || a.Year >= yearMin);

            if (filters?.YearMax is int yearMax)
                items = items.Where(a => a.Year is null || a.Year <= yearMax);

            if (!string.IsNullOrWhiteSpace(filters?.Language))
                items = items.Where(a =>
                    string.IsNullOrWhiteSpace(a.Language)
                    || a.Language.Equals(filters.Language, StringComparison.OrdinalIgnoreCase));

            if (filters?.MinCitations is int minCitations)
                items = items.Where(a => a.CitationCount >= minCitations);

            return items.Where(a => !string.IsNullOrWhiteSpace(a.PdfUrl));
        }

        // Relevance to the user's query decides the order; citations only break ties.
        // A minimum-coverage floor drops results that never mention the query terms
        // in their title or abstract (e.g. full-text-only matches from Europe PMC).
        var queryTerms = TokenizeQuery(query);

        var ranked = results
            .SelectMany(ApplyFilters)
            .Select(c => (Candidate: c, Score: RelevanceScore(c, queryTerms, query)))
            .Where(x => x.Score.Coverage >= MinimumCoverage(queryTerms.Count))
            .OrderByDescending(x => x.Score.Value)
            .ThenByDescending(x => x.Candidate.CitationCount)
            .ThenByDescending(x => x.Candidate.Year ?? 0)
            .Select(x => x.Candidate);

        return Deduplicate(ranked).ToList();
    }

    private static double MinimumCoverage(int termCount) => termCount switch
    {
        <= 1 => 1.0,   // single-term query: the term must appear
        2 => 0.5,      // at least one of two terms
        _ => 0.6,      // majority of terms for longer queries
    };

    private static (double Value, double Coverage) RelevanceScore(
        AcademicArticleCandidate candidate,
        IReadOnlyList<string> terms,
        string rawQuery)
    {
        if (terms.Count == 0)
            return (Math.Log10(candidate.CitationCount + 1), 1.0);

        var title = Normalize(candidate.Title);
        var abstractText = Normalize(candidate.Abstract ?? string.Empty);

        var inTitle = 0;
        var inAbstract = 0;
        foreach (var term in terms)
        {
            if (title.Contains(term, StringComparison.Ordinal)) inTitle++;
            else if (abstractText.Contains(term, StringComparison.Ordinal)) inAbstract++;
        }

        var coverage = (inTitle + inAbstract) / (double)terms.Count;
        var score =
            3.0 * (inTitle / (double)terms.Count)
            + 1.0 * (inAbstract / (double)terms.Count);

        // Exact phrase is the strongest signal for multi-word queries.
        var phrase = Normalize(rawQuery);
        if (terms.Count > 1 && phrase.Length > 3)
        {
            if (title.Contains(phrase, StringComparison.Ordinal)) score += 3.0;
            else if (abstractText.Contains(phrase, StringComparison.Ordinal)) score += 1.5;
        }

        // Citations as a mild boost, never the driver.
        score += Math.Min(1.0, Math.Log10(candidate.CitationCount + 1) * 0.25);

        return (score, coverage);
    }

    private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
    {
        // pt
        "de", "da", "do", "das", "dos", "em", "no", "na", "nos", "nas", "um", "uma",
        "para", "por", "com", "sem", "sobre", "entre", "que", "como", "mais", "menos",
        "e", "ou", "a", "o", "as", "os", "ao", "aos", "seu", "sua",
        // en
        "the", "of", "in", "on", "at", "to", "for", "with", "and", "or", "an", "is",
        "are", "by", "from", "about", "into", "how", "what", "why", "a",
    };

    private static List<string> TokenizeQuery(string query)
    {
        return Normalize(query)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2 && !StopWords.Contains(t))
            .Distinct()
            .ToList();
    }

    /// <summary>Lowercase, strip diacritics and collapse non-alphanumerics to spaces.</summary>
    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var formD = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;
            sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
        }

        return string.Join(' ',
            sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private async Task<IReadOnlyList<AcademicArticleCandidate>> SafeSearch(
        Func<Task<IReadOnlyList<AcademicArticleCandidate>>> search,
        string sourceName)
    {
        try
        {
            return await search();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Academic search failed for source {Source}", sourceName);
            return [];
        }
    }

    private static IEnumerable<AcademicArticleCandidate> Deduplicate(
        IEnumerable<AcademicArticleCandidate> candidates)
    {
        var seenDois = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate.Doi))
            {
                if (!seenDois.Add(candidate.Doi))
                    continue;
            }
            else
            {
                var titleKey = candidate.Title.Trim().ToLowerInvariant();
                if (!seenTitles.Add(titleKey))
                    continue;
            }

            yield return candidate;
        }
    }
}
