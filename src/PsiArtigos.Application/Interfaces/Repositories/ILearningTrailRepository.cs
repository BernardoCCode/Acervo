using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface ILearningTrailRepository
{
    Task<LearningTrail?> GetByIdAsync(
        LearningTrailId id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearningTrail>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(LearningTrail trail, CancellationToken cancellationToken = default);
}
