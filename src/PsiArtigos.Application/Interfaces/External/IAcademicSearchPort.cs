using PsiArtigos.Application.DTOs.Search;
using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.Interfaces;

public interface IAcademicSearchPort
{
    Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        SearchFiltersRequest? filters,
        CancellationToken cancellationToken = default);
}

public sealed record AcademicArticleCandidate(
    string Title,
    string? Abstract,
    IReadOnlyList<string> Authors,
    string? Venue,
    int? Year,
    string? Doi,
    string? Url,
    string? PdfUrl,
    string? Language,
    int CitationCount,
    SourceSystem PrimarySource,
    string ExternalId,
    StudyType StudyType);
