using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class LearningTrailRepository : ILearningTrailRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public LearningTrailRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LearningTrail?> GetByIdAsync(
        LearningTrailId id,
        CancellationToken cancellationToken = default)
        => _dbContext.LearningTrails.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<LearningTrail>> ListByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
        => await _dbContext.LearningTrails
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(LearningTrail trail, CancellationToken cancellationToken = default)
        => await _dbContext.LearningTrails.AddAsync(trail, cancellationToken);
}
