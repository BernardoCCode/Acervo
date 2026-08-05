using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Search;

public sealed record SearchArticlesRequest(
    string Query,
    SearchFiltersRequest? Filters = null);

public sealed record SearchFiltersRequest(
    int? YearMin = null,
    int? YearMax = null,
    string? Language = null,
    StudyType? StudyType = null,
    int? MinCitations = null,
    IReadOnlyList<SourceSystem>? Sources = null);
