using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Favorite>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);

    void Remove(Favorite favorite);
}
