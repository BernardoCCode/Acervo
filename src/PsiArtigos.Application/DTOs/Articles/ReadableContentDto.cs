using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Articles;

public sealed record ReadableContentDto(
    Guid ArticleId,
    string Title,
    string Body,
    IReadOnlyList<string> Paragraphs,
    ReadableContentSource Source,
    int? PageCount,
    bool IsFallback,
    string? Message);
