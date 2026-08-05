using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

public sealed class CrossrefSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly AcademicSearchOptions _options;

    public CrossrefSearchClient(HttpClient httpClient, IOptions<AcademicSearchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var url = $"works?query={Uri.EscapeDataString(query)}&rows={_options.MaxResultsPerSource}";
        var response = await _httpClient.GetFromJsonAsync<CrossrefResponse>(url, cancellationToken);
        var items = response?.Message?.Items;
        if (items is null)
            return [];

        return items
            .Where(i => i.Title is { Count: > 0 })
            .Select(Map)
            .ToList();
    }

    private static AcademicArticleCandidate Map(CrossrefItem item)
    {
        var title = item.Title![0];
        var authors = item.Author?
            .Select(a => $"{a.Given} {a.Family}".Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList() ?? [];

        var year = item.Published?.DateParts?.FirstOrDefault()?.FirstOrDefault();

        return new AcademicArticleCandidate(
            Title: title,
            Abstract: null,
            Authors: authors,
            Venue: item.ContainerTitle?.FirstOrDefault(),
            Year: year,
            Doi: item.Doi,
            Url: string.IsNullOrWhiteSpace(item.Doi) ? null : $"https://doi.org/{item.Doi}",
            PdfUrl: null,
            Language: item.Language,
            CitationCount: item.IsReferencedByCount ?? 0,
            PrimarySource: SourceSystem.Crossref,
            ExternalId: item.Doi ?? Guid.NewGuid().ToString("N"),
            StudyType: StudyType.Unknown);
    }

    private sealed class CrossrefResponse
    {
        [JsonPropertyName("message")]
        public CrossrefMessage? Message { get; set; }
    }

    private sealed class CrossrefMessage
    {
        [JsonPropertyName("items")]
        public List<CrossrefItem>? Items { get; set; }
    }

    private sealed class CrossrefItem
    {
        [JsonPropertyName("DOI")]
        public string? Doi { get; set; }

        [JsonPropertyName("title")]
        public List<string>? Title { get; set; }

        [JsonPropertyName("author")]
        public List<CrossrefAuthor>? Author { get; set; }

        [JsonPropertyName("container-title")]
        public List<string>? ContainerTitle { get; set; }

        [JsonPropertyName("published")]
        public CrossrefDate? Published { get; set; }

        [JsonPropertyName("is-referenced-by-count")]
        public int? IsReferencedByCount { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }

    private sealed class CrossrefAuthor
    {
        [JsonPropertyName("given")]
        public string? Given { get; set; }

        [JsonPropertyName("family")]
        public string? Family { get; set; }
    }

    private sealed class CrossrefDate
    {
        [JsonPropertyName("date-parts")]
        public List<List<int>>? DateParts { get; set; }
    }
}
