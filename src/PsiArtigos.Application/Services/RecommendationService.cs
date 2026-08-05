using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Recommendations;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class RecommendationService
{
    private readonly IRecommendationRepository _recommendations;
    private readonly IArticleRepository _articles;
    private readonly RecommendationEngineService _engine;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        IRecommendationRepository recommendations,
        IArticleRepository articles,
        RecommendationEngineService engine,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IServiceScopeFactory scopeFactory,
        IMemoryCache memoryCache,
        ILogger<RecommendationService> logger)
    {
        _recommendations = recommendations;
        _articles = articles;
        _engine = engine;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _scopeFactory = scopeFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RecommendationDto>> ListAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var recommendations = await _recommendations.ListActiveByUserAsync(
            userId,
            take,
            cancellationToken);

        // Never block the home/library request on the multi-source refresh pipeline.
        if (recommendations.Count == 0)
        {
            ScheduleBackgroundRefresh(userId);
            return [];
        }

        return await MapAsync(recommendations, cancellationToken);
    }

    public Task<int> RefreshAsync(CancellationToken cancellationToken = default)
        => _engine.RefreshAsync(cancellationToken);

    public async Task DismissAsync(
        Guid recommendationId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var recommendation = await _recommendations.GetByIdAsync(
            RecommendationId.From(recommendationId),
            cancellationToken);

        if (recommendation is null)
            throw NotFoundException.For<Recommendation>(recommendationId);

        recommendation.EnsureOwnedBy(userId);
        recommendation.Dismiss();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RecommendationDto>> MapAsync(
        IReadOnlyList<Recommendation> recommendations,
        CancellationToken cancellationToken)
    {
        var articleIds = recommendations.Select(r => r.ArticleId).Distinct();
        var articles = await _articles.GetByIdsAsync(articleIds, cancellationToken);
        var articlesById = articles.ToDictionary(a => a.Id);

        return recommendations
            .Where(r => articlesById.ContainsKey(r.ArticleId))
            .Select(r => new RecommendationDto(
                r.Id.Value,
                r.Reason,
                r.Score,
                r.Explanation,
                r.SourceArticleId?.Value,
                r.TopicScore,
                r.EngagementScore,
                r.QualityScore,
                r.FreshnessScore,
                r.ExpiresAtUtc,
                articlesById[r.ArticleId].ToDto()))
            .ToList();
    }

    private void ScheduleBackgroundRefresh(UserId userId)
    {
        var lockKey = $"rec-refresh:{userId.Value:D}";
        if (_memoryCache.TryGetValue(lockKey, out _))
            return;

        using (var entry = _memoryCache.CreateEntry(lockKey))
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
            entry.Value = true;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var engine = scope.ServiceProvider.GetRequiredService<RecommendationEngineService>();
                await engine.RefreshForUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background recommendation refresh failed for {UserId}", userId.Value);
            }
            finally
            {
                _memoryCache.Remove(lockKey);
            }
        });
    }
}
