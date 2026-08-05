using System.Xml.Linq;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AcademicSearch;

public sealed class ArxivSearchClient
{
    private readonly HttpClient _httpClient;
    private readonly AcademicSearchOptions _options;

    public ArxivSearchClient(HttpClient httpClient, IOptions<AcademicSearchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        // Title/abstract only — "all:" also matches comments and journal refs.
        var phrase = $"\"{query.Trim().Replace("\"", string.Empty)}\"";
        var searchQuery = $"ti:{phrase} OR abs:{phrase}";
        var url =
            $"api/query?search_query={Uri.EscapeDataString(searchQuery)}&start=0&max_results={_options.MaxResultsPerSource}";

        await using var stream = await _httpClient.GetStreamAsync(url, cancellationToken);
        var document = XDocument.Load(stream);
        XNamespace atom = "http://www.w3.org/2005/Atom";

        return document.Root?
            .Elements(atom + "entry")
            .Select(entry =>
            {
                var id = entry.Element(atom + "id")?.Value ?? Guid.NewGuid().ToString("N");
                var title = (entry.Element(atom + "title")?.Value ?? string.Empty)
                    .Replace('\n', ' ')
                    .Trim();
                var summary = entry.Element(atom + "summary")?.Value?.Trim();
                var published = entry.Element(atom + "published")?.Value;
                int? year = DateTime.TryParse(published, out var date) ? date.Year : null;

                var authors = entry.Elements(atom + "author")
                    .Select(a => a.Element(atom + "name")?.Value)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Cast<string>()
                    .ToList();

                var pdf = entry.Elements(atom + "link")
                    .FirstOrDefault(l => (string?)l.Attribute("title") == "pdf")
                    ?.Attribute("href")?.Value;

                return new AcademicArticleCandidate(
                    Title: title,
                    Abstract: summary,
                    Authors: authors,
                    Venue: "arXiv",
                    Year: year,
                    Doi: null,
                    Url: id,
                    PdfUrl: pdf,
                    Language: "en",
                    CitationCount: 0,
                    PrimarySource: SourceSystem.ArXiv,
                    ExternalId: id,
                    StudyType: StudyType.Unknown);
            })
            .Where(c => !string.IsNullOrWhiteSpace(c.Title))
            .ToList()
            ?? [];
    }
}
