using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface ISearchQueryRepository
{
    Task<IReadOnlyList<SearchQuery>> ListRecentByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default);

    Task AddAsync(SearchQuery searchQuery, CancellationToken cancellationToken = default);
}
