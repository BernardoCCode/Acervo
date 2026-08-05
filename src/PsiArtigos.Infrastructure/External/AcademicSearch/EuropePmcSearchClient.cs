using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

/// <summary>
/// Europe PMC — strong coverage of PubMed Central open PDFs (psychology, health).
/// </summary>
public sealed class EuropePmcSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly AcademicSearchOptions _options;

    public EuropePmcSearchClient(HttpClient httpClient, IOptions<AcademicSearchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Clamp(_options.MaxResultsPerSource, 10, 50);
        // Scope matching to title/abstract; full-text matches produce off-topic results.
        var trimmed = query.Trim().Replace("\"", string.Empty);
        var q = $"(TITLE:({trimmed}) OR ABSTRACT:({trimmed})) AND HAS_PDF:y";
        var url =
            $"search?query={Uri.EscapeDataString(q)}"
            + $"&format=json&pageSize={pageSize}&resultType=core";

        var response = await _httpClient.GetFromJsonAsync<EuropePmcResponse>(url, cancellationToken);
        var results = response?.ResultList?.Result;
        if (results is null)
            return [];

        return results
            .Select(Map)
            .Where(c => !string.IsNullOrWhiteSpace(c.Title) && !string.IsNullOrWhiteSpace(c.PdfUrl))
            .ToList();
    }

    private static AcademicArticleCandidate Map(EuropePmcResult item)
    {
        var pdfUrl = item.FullTextUrlList?.FullTextUrl?
            .Where(u =>
                string.Equals(u.DocumentStyle, "pdf", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(u.Url))
            .OrderBy(u => PreferPdfHost(u.Site))
            .Select(u => u.Url)
            .FirstOrDefault();

        var htmlUrl = item.FullTextUrlList?.FullTextUrl?
            .FirstOrDefault(u =>
                string.Equals(u.DocumentStyle, "html", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(u.Url))
            ?.Url;

        var authors = (item.AuthorString ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Take(12)
            .ToList();

        int.TryParse(item.PubYear, out var year);
        var yearValue = year > 0 ? year : (int?)null;

        return new AcademicArticleCandidate(
            Title: CleanHtml(item.Title) ?? "Untitled",
            Abstract: CleanHtml(item.AbstractText),
            Authors: authors,
            Venue: item.JournalTitle,
            Year: yearValue,
            Doi: item.Doi,
            Url: htmlUrl
                ?? (item.Pmid is not null ? $"https://europepmc.org/article/MED/{item.Pmid}" : null),
            PdfUrl: pdfUrl,
            Language: null,
            CitationCount: item.CitedByCount ?? 0,
            PrimarySource: SourceSystem.PubMed,
            ExternalId: item.Id ?? item.Pmid ?? Guid.NewGuid().ToString("N"),
            StudyType: StudyType.Unknown);
    }

    private static int PreferPdfHost(string? site) => site switch
    {
        "Europe_PMC" => 0,
        "PubMedCentral" => 1,
        _ => 2
    };

    private static string? CleanHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value
            .Replace("<i>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("</i>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("<b>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("</b>", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&amp;", "&", StringComparison.Ordinal)
            .Trim();
    }

    private sealed class EuropePmcResponse
    {
        [JsonPropertyName("resultList")]
        public EuropePmcResultList? ResultList { get; set; }
    }

    private sealed class EuropePmcResultList
    {
        [JsonPropertyName("result")]
        public List<EuropePmcResult>? Result { get; set; }
    }

    private sealed class EuropePmcResult
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("pmid")]
        public string? Pmid { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("authorString")]
        public string? AuthorString { get; set; }

        [JsonPropertyName("journalTitle")]
        public string? JournalTitle { get; set; }

        [JsonPropertyName("pubYear")]
        public string? PubYear { get; set; }

        [JsonPropertyName("doi")]
        public string? Doi { get; set; }

        [JsonPropertyName("abstractText")]
        public string? AbstractText { get; set; }

        [JsonPropertyName("citedByCount")]
        public int? CitedByCount { get; set; }

        [JsonPropertyName("fullTextUrlList")]
        public EuropePmcFullTextList? FullTextUrlList { get; set; }
    }

    private sealed class EuropePmcFullTextList
    {
        [JsonPropertyName("fullTextUrl")]
        public List<EuropePmcFullTextUrl>? FullTextUrl { get; set; }
    }

    private sealed class EuropePmcFullTextUrl
    {
        [JsonPropertyName("availability")]
        public string? Availability { get; set; }

        [JsonPropertyName("availabilityCode")]
        public string? AvailabilityCode { get; set; }

        [JsonPropertyName("documentStyle")]
        public string? DocumentStyle { get; set; }

        [JsonPropertyName("site")]
        public string? Site { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
