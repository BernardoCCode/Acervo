using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IReadingSessionRepository
{
    Task<ReadingSession?> GetByIdAsync(
        ReadingSessionId id,
        CancellationToken cancellationToken = default);

    Task<ReadingSession?> GetByUserAndArticleAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an existing session or creates one. Safe under concurrent open requests
    /// (e.g. React StrictMode double-mount).
    /// </summary>
    Task<ReadingSession> GetOrCreateAsync(
        UserId userId,
        ArticleId articleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReadingSession>> ListRecentByUserAsync(
        UserId userId,
        int take,
        CancellationToken cancellationToken = default);

    Task AddAsync(ReadingSession session, CancellationToken cancellationToken = default);
}
