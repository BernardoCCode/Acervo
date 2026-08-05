using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class RecommendationRepository : IRecommendationRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public RecommendationRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Recommendation>> ListActiveByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default)
        => await _dbContext.Recommendations
            .Where(r => r.UserId == userId && !r.IsDismissed && r.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(r => r.Score)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<Recommendation?> GetByIdAsync(
        RecommendationId id,
        CancellationToken cancellationToken = default)
        => _dbContext.Recommendations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Recommendation>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
        => await _dbContext.Recommendations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task RemoveActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var active = await _dbContext.Recommendations
            .Where(r => r.UserId == userId && !r.IsDismissed)
            .ToListAsync(cancellationToken);
        _dbContext.Recommendations.RemoveRange(active);
    }

    public async Task AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
        => await _dbContext.Recommendations.AddAsync(recommendation, cancellationToken);
}
