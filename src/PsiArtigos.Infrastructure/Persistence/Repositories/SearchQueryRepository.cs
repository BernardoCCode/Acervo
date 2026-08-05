using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class SearchQueryRepository : ISearchQueryRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public SearchQueryRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SearchQuery>> ListRecentByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default)
        => await _dbContext.SearchQueries
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.LastAccessedAtUtc ?? q.ExecutedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SearchQuery searchQuery, CancellationToken cancellationToken = default)
        => await _dbContext.SearchQueries.AddAsync(searchQuery, cancellationToken);
}
