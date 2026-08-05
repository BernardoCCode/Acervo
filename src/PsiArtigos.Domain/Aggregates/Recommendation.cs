using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class Recommendation : AggregateRoot<RecommendationId>
{
    public UserId UserId { get; private set; }
    public ArticleId ArticleId { get; private set; }
    public RecommendationReason Reason { get; private set; }
    public double Score { get; private set; }
    public string? Explanation { get; private set; }
    public ArticleId? SourceArticleId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public double TopicScore { get; private set; }
    public double EngagementScore { get; private set; }
    public double QualityScore { get; private set; }
    public double FreshnessScore { get; private set; }
    public bool IsDismissed { get; private set; }

    private Recommendation()
    {
    }

    public static Recommendation Create(
        UserId userId,
        ArticleId articleId,
        RecommendationReason reason,
        double score,
        string? explanation = null,
        ArticleId? sourceArticleId = null,
        double topicScore = 0,
        double engagementScore = 0,
        double qualityScore = 0,
        double freshnessScore = 0,
        DateTime? expiresAtUtc = null,
        DateTime? createdAtUtc = null)
    {
        if (!Enum.IsDefined(reason))
            throw new DomainException("Invalid recommendation reason.");

        if (score is < 0 or > 1)
            throw new DomainException("Recommendation score must be between 0 and 1.");

        var now = createdAtUtc ?? DateTime.UtcNow;
        ValidateComponent(topicScore);
        ValidateComponent(engagementScore);
        ValidateComponent(qualityScore);
        ValidateComponent(freshnessScore);

        var recommendation = new Recommendation
        {
            Id = RecommendationId.New(),
            UserId = userId,
            ArticleId = articleId,
            Reason = reason,
            Score = Math.Round(score, 4),
            Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation.Trim(),
            SourceArticleId = sourceArticleId,
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc ?? now.AddDays(7),
            TopicScore = Math.Round(topicScore, 4),
            EngagementScore = Math.Round(engagementScore, 4),
            QualityScore = Math.Round(qualityScore, 4),
            FreshnessScore = Math.Round(freshnessScore, 4),
            IsDismissed = false
        };

        recommendation.Raise(new ArticleRecommended(
            recommendation.Id,
            userId,
            articleId,
            reason,
            now));

        return recommendation;
    }

    public void Dismiss()
    {
        IsDismissed = true;
    }

    public void EnsureOwnedBy(UserId userId)
    {
        if (UserId != userId)
            throw new DomainException("Recommendation does not belong to this user.");
    }

    private static void ValidateComponent(double score)
    {
        if (score is < 0 or > 1)
            throw new DomainException("Recommendation score components must be between 0 and 1.");
    }
}
