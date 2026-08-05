using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

public sealed class OpenAlexSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly AcademicSearchOptions _options;

    public OpenAlexSearchClient(HttpClient httpClient, IOptions<AcademicSearchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var mailto = string.IsNullOrWhiteSpace(_options.OpenAlexMailto)
            ? string.Empty
            : $"&mailto={Uri.EscapeDataString(_options.OpenAlexMailto)}";

        var perPage = Math.Clamp(_options.MaxResultsPerSource, 10, 50);

        // OpenAlex's top-level ?search= now maps to fulltext-only and misses most
        // psychology/humanities OA. Prefer title/abstract + works that have a PDF.
        var q = Uri.EscapeDataString(EscapeFilterValue(query));
        var url =
            $"works?filter=title_and_abstract.search:{q},type:article|preprint|review,has_content.pdf:true"
            + $"&per-page={perPage}"
            + "&sort=relevance_score:desc"
            + mailto;

        var response = await _httpClient.GetFromJsonAsync<OpenAlexResponse>(url, cancellationToken);
        if (response?.Results is null || response.Results.Count == 0)
        {
            // Fallback: OA with title/abstract match (still require a resolvable PDF URL).
            url =
                $"works?filter=title_and_abstract.search:{q},type:article|preprint|review,is_oa:true"
                + $"&per-page={perPage}"
                + "&sort=cited_by_count:desc"
                + mailto;

            response = await _httpClient.GetFromJsonAsync<OpenAlexResponse>(url, cancellationToken);
        }

        if (response?.Results is null)
            return [];

        return response.Results
            .Select(Map)
            .Where(c => !string.IsNullOrWhiteSpace(c.Title) && !string.IsNullOrWhiteSpace(c.PdfUrl))
            .ToList();
    }

    private static string EscapeFilterValue(string value)
    {
        // Commas/colons break OpenAlex filter syntax; keep the query readable.
        return value
            .Replace(',', ' ')
            .Replace(':', ' ')
            .Trim();
    }

    private static AcademicArticleCandidate Map(OpenAlexWork work)
    {
        var authors = work.Authorships?
            .Select(a => a.Author?.DisplayName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Cast<string>()
            .ToList() ?? [];

        var doi = work.Doi?.Replace("https://doi.org/", string.Empty, StringComparison.OrdinalIgnoreCase);
        var pdfUrl = work.BestOaLocation?.PdfUrl
            ?? work.PrimaryLocation?.PdfUrl
            ?? work.Locations?
                .Select(l => l.PdfUrl)
                .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));

        var landing = work.BestOaLocation?.LandingPageUrl
            ?? work.PrimaryLocation?.LandingPageUrl
            ?? work.Id;

        return new AcademicArticleCandidate(
            Title: work.Title!,
            Abstract: ReconstructAbstract(work.AbstractInvertedIndex),
            Authors: authors,
            Venue: work.PrimaryLocation?.Source?.DisplayName
                ?? work.BestOaLocation?.Source?.DisplayName,
            Year: work.PublicationYear,
            Doi: doi,
            Url: landing,
            PdfUrl: pdfUrl,
            Language: work.Language,
            CitationCount: work.CitedByCount ?? 0,
            PrimarySource: SourceSystem.OpenAlex,
            ExternalId: work.Id ?? Guid.NewGuid().ToString("N"),
            StudyType: StudyType.Unknown);
    }

    private static string? ReconstructAbstract(Dictionary<string, List<int>>? inverted)
    {
        if (inverted is null || inverted.Count == 0)
            return null;

        var maxPos = -1;
        foreach (var positions in inverted.Values)
        {
            foreach (var pos in positions)
                if (pos > maxPos) maxPos = pos;
        }

        if (maxPos < 0)
            return null;

        var words = new string?[maxPos + 1];
        foreach (var (word, positions) in inverted)
        {
            foreach (var pos in positions)
            {
                if (pos >= 0 && pos < words.Length)
                    words[pos] = word;
            }
        }

        var text = string.Join(' ', words.Where(w => !string.IsNullOrWhiteSpace(w)));
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private sealed class OpenAlexResponse
    {
        [JsonPropertyName("results")]
        public List<OpenAlexWork>? Results { get; set; }
    }

    private sealed class OpenAlexWork
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("doi")]
        public string? Doi { get; set; }

        [JsonPropertyName("publication_year")]
        public int? PublicationYear { get; set; }

        [JsonPropertyName("cited_by_count")]
        public int? CitedByCount { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("abstract_inverted_index")]
        public Dictionary<string, List<int>>? AbstractInvertedIndex { get; set; }

        [JsonPropertyName("authorships")]
        public List<OpenAlexAuthorship>? Authorships { get; set; }

        [JsonPropertyName("primary_location")]
        public OpenAlexLocation? PrimaryLocation { get; set; }

        [JsonPropertyName("best_oa_location")]
        public OpenAlexLocation? BestOaLocation { get; set; }

        [JsonPropertyName("locations")]
        public List<OpenAlexLocation>? Locations { get; set; }
    }

    private sealed class OpenAlexAuthorship
    {
        [JsonPropertyName("author")]
        public OpenAlexAuthor? Author { get; set; }
    }

    private sealed class OpenAlexAuthor
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }

    private sealed class OpenAlexLocation
    {
        [JsonPropertyName("pdf_url")]
        public string? PdfUrl { get; set; }

        [JsonPropertyName("landing_page_url")]
        public string? LandingPageUrl { get; set; }

        [JsonPropertyName("source")]
        public OpenAlexSource? Source { get; set; }
    }

    private sealed class OpenAlexSource
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
