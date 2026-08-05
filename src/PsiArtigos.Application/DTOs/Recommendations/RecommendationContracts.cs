using PsiArtigos.Application.DTOs.Articles;
using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Recommendations;

public sealed record RecommendationDto(
    Guid Id,
    RecommendationReason Reason,
    double Score,
    string? Explanation,
    Guid? SourceArticleId,
    double TopicScore,
    double EngagementScore,
    double QualityScore,
    double FreshnessScore,
    DateTime ExpiresAtUtc,
    ArticleDto Article);

public sealed record RecommendationRefreshDto(int GeneratedCount);
