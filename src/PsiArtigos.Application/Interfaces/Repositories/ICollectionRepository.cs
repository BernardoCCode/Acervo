using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(CollectionId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Collection>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(
        UserId userId,
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(Collection collection, CancellationToken cancellationToken = default);
}
