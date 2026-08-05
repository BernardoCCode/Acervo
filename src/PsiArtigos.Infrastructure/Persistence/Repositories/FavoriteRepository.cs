using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public FavoriteRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Favorite?> GetAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default)
        => _dbContext.Favorites.FirstOrDefaultAsync(
            f => f.UserId == userId && f.ArticleId == articleId,
            cancellationToken);

    public async Task<IReadOnlyList<Favorite>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
        => await _dbContext.Favorites
            .Where(f => f.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        => await _dbContext.Favorites.AddAsync(favorite, cancellationToken);

    public void Remove(Favorite favorite)
        => _dbContext.Favorites.Remove(favorite);
}
