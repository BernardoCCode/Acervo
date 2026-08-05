namespace PsiArtigos.Application.Interfaces;

public sealed record RecommendationProfileInput(
    IReadOnlyList<string> StrongArticleTitles,
    IReadOnlyList<string> RecentSearches,
    IReadOnlyList<string> DeclaredInterests);

public sealed record RecommendationProfilePlan(
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> SearchQueries,
    string Summary);

public interface IAiRecommendationPort
{
    Task<RecommendationProfilePlan> AnalyzeAsync(
        RecommendationProfileInput input,
        CancellationToken cancellationToken = default);
}
