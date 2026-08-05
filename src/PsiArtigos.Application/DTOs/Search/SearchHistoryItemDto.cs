namespace PsiArtigos.Application.DTOs.Search;

public sealed record SearchHistoryItemDto(
    Guid Id,
    string Query,
    int ResultCount,
    DateTime ExecutedAtUtc,
    DateTime? LastAccessedAtUtc);
