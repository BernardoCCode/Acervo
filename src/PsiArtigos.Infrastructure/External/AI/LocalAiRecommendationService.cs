using PsiArtigos.Application.Interfaces;

namespace PsiArtigos.Infrastructure.External.AI;

public sealed class LocalAiRecommendationService : IAiRecommendationPort
{
    public Task<RecommendationProfilePlan> AnalyzeAsync(
        RecommendationProfileInput input,
        CancellationToken cancellationToken = default)
    {
        var seeds = input.DeclaredInterests
            .Concat(input.RecentSearches)
            .Concat(input.StrongArticleTitles)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();
        return Task.FromResult(new RecommendationProfilePlan(
            seeds.Take(5).ToList(),
            seeds.Select(x => $"{x} systematic review recent advances").Take(5).ToList(),
            "Perfil calculado a partir das leituras, favoritos e buscas recentes."));
    }
}
