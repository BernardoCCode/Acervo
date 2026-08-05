using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Insights;

public sealed record GenerateInsightRequest(
    Guid ArticleId,
    InsightType Type,
    string? SourceLanguage = null,
    string? TargetLanguage = null,
    string? FocusText = null);

public sealed record InsightDto(
    Guid Id,
    Guid ArticleId,
    InsightType Type,
    string Content,
    string? SourceLanguage,
    string? TargetLanguage,
    DateTime CreatedAtUtc);
