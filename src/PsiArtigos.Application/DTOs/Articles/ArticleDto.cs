using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Articles;

public sealed record ArticleDto(
    Guid Id,
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
    StudyType StudyType,
    IReadOnlyList<string> Topics);
