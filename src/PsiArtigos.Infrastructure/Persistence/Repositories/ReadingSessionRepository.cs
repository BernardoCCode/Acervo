using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class ReadingSessionRepository : IReadingSessionRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public ReadingSessionRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReadingSession?> GetByIdAsync(
        ReadingSessionId id,
        CancellationToken cancellationToken = default)
        => _dbContext.ReadingSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<ReadingSession?> GetByUserAndArticleAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default)
        => _dbContext.ReadingSessions.FirstOrDefaultAsync(
            s => s.UserId == userId && s.ArticleId == articleId,
            cancellationToken);

    public async Task<ReadingSession> GetOrCreateAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByUserAndArticleAsync(userId, articleId, cancellationToken);
        if (existing is not null)
            return existing;

        var session = ReadingSession.Open(userId, articleId);
        await _dbContext.ReadingSessions.AddAsync(session, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return session;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(session).State = EntityState.Detached;

            var raced = await GetByUserAndArticleAsync(userId, articleId, cancellationToken);
            if (raced is not null)
                return raced;

            throw;
        }
    }

    public async Task<IReadOnlyList<ReadingSession>> ListRecentByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default)
        => await _dbContext.ReadingSessions
            .Include("_highlights")
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastOpenedAtUtc)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ReadingSession session, CancellationToken cancellationToken = default)
        => await _dbContext.ReadingSessions.AddAsync(session, cancellationToken);
}
