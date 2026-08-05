using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

/// <summary>
/// Semantic Scholar — papers with openAccessPdf for in-app extraction.
/// </summary>
public sealed class SemanticScholarSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly AcademicSearchOptions _options;

    public SemanticScholarSearchClient(HttpClient httpClient, IOptions<AcademicSearchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(_options.MaxResultsPerSource, 10, 50);
        var fields = "title,abstract,year,venue,authors,citationCount,externalIds,openAccessPdf,url";
        var url =
            $"paper/search?query={Uri.EscapeDataString(query.Trim())}"
            + $"&limit={limit}"
            + $"&fields={fields}";

        using var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
        // Unauthenticated S2 frequently returns 429 — skip quietly.
        if (httpResponse.StatusCode is System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.Unauthorized
            or System.Net.HttpStatusCode.Forbidden)
        {
            return [];
        }

        httpResponse.EnsureSuccessStatusCode();
        var response = await httpResponse.Content.ReadFromJsonAsync<SemanticScholarResponse>(
            cancellationToken: cancellationToken);

        if (response?.Data is null)
            return [];

        return response.Data
            .Select(Map)
            .Where(c => !string.IsNullOrWhiteSpace(c.Title) && !string.IsNullOrWhiteSpace(c.PdfUrl))
            .ToList();
    }

    private static AcademicArticleCandidate Map(SemanticScholarPaper paper)
    {
        var authors = paper.Authors?
            .Select(a => a.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Cast<string>()
            .Take(12)
            .ToList() ?? [];

        return new AcademicArticleCandidate(
            Title: paper.Title!,
            Abstract: paper.Abstract,
            Authors: authors,
            Venue: paper.Venue,
            Year: paper.Year,
            Doi: paper.ExternalIds?.Doi,
            Url: paper.Url,
            PdfUrl: paper.OpenAccessPdf?.Url,
            Language: null,
            CitationCount: paper.CitationCount ?? 0,
            PrimarySource: SourceSystem.Scholar,
            ExternalId: paper.PaperId ?? Guid.NewGuid().ToString("N"),
            StudyType: StudyType.Unknown);
    }

    private sealed class SemanticScholarResponse
    {
        [JsonPropertyName("data")]
        public List<SemanticScholarPaper>? Data { get; set; }
    }

    private sealed class SemanticScholarPaper
    {
        [JsonPropertyName("paperId")]
        public string? PaperId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("abstract")]
        public string? Abstract { get; set; }

        [JsonPropertyName("year")]
        public int? Year { get; set; }

        [JsonPropertyName("venue")]
        public string? Venue { get; set; }

        [JsonPropertyName("citationCount")]
        public int? CitationCount { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("authors")]
        public List<SemanticScholarAuthor>? Authors { get; set; }

        [JsonPropertyName("externalIds")]
        public SemanticScholarExternalIds? ExternalIds { get; set; }

        [JsonPropertyName("openAccessPdf")]
        public SemanticScholarPdf? OpenAccessPdf { get; set; }
    }

    private sealed class SemanticScholarAuthor
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class SemanticScholarExternalIds
    {
        [JsonPropertyName("DOI")]
        public string? Doi { get; set; }
    }

    private sealed class SemanticScholarPdf
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
