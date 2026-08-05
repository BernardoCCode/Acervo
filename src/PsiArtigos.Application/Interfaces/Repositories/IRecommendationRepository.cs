using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IRecommendationRepository
{
    Task<IReadOnlyList<Recommendation>> ListActiveByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default);

    Task<Recommendation?> GetByIdAsync(
        RecommendationId id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recommendation>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task RemoveActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default);
}
