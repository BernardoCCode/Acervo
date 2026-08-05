using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class GeneratedInsightRepository : IGeneratedInsightRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public GeneratedInsightRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<GeneratedInsight?> GetByIdAsync(
        InsightId id,
        CancellationToken cancellationToken = default)
        => _dbContext.GeneratedInsights.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<GeneratedInsight?> GetLatestAsync(
        UserId userId,
        ArticleId articleId,
        InsightType type,
        CancellationToken cancellationToken = default)
        => _dbContext.GeneratedInsights
            .Where(i => i.UserId == userId && i.ArticleId == articleId && i.Type == type)
            .OrderByDescending(i => i.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(GeneratedInsight insight, CancellationToken cancellationToken = default)
        => await _dbContext.GeneratedInsights.AddAsync(insight, cancellationToken);
}
