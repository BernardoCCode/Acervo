using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class CollectionRepository : ICollectionRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public CollectionRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Collection?> GetByIdAsync(CollectionId id, CancellationToken cancellationToken = default)
        => _dbContext.Collections.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Collection>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
        => await _dbContext.Collections
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsWithNameAsync(
        UserId userId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return _dbContext.Collections.AnyAsync(
            c => c.UserId == userId && c.Name == normalized,
            cancellationToken);
    }

    public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default)
        => await _dbContext.Collections.AddAsync(collection, cancellationToken);
}
