using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Reading;

public sealed record ReadingSessionDto(
    Guid Id,
    Guid ArticleId,
    double ProgressPercent,
    int? PageNumber,
    bool IsCompleted,
    DateTime LastOpenedAtUtc,
    int OpenCount,
    int ActiveReadingSeconds,
    IReadOnlyList<HighlightDto> Highlights);

public sealed record HighlightDto(
    Guid Id,
    int StartOffset,
    int EndOffset,
    int? PageNumber,
    string QuotedText,
    HighlightColor Color,
    IReadOnlyList<AnnotationDto> Annotations);

public sealed record AnnotationDto(
    Guid Id,
    string Note,
    DateTime CreatedAtUtc);

public sealed record UpdateReadingProgressRequest(
    Guid SessionId,
    double Percent,
    int? PageNumber = null,
    int? CharacterOffset = null,
    int ActiveSeconds = 0);

public sealed record AddHighlightRequest(
    Guid SessionId,
    int StartOffset,
    int EndOffset,
    string QuotedText,
    HighlightColor Color = HighlightColor.Yellow,
    int? PageNumber = null,
    string? Note = null);

public sealed record ReaderPreferencesDto(
    bool DarkMode,
    int FontSize,
    string? PreferredTranslationLanguage);

public sealed record UpdateReaderPreferencesRequest(
    bool DarkMode,
    int FontSize,
    string? PreferredTranslationLanguage = null);
