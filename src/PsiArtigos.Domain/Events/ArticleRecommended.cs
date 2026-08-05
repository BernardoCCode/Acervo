using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record ArticleRecommended(
    RecommendationId RecommendationId,
    UserId UserId,
    ArticleId ArticleId,
    RecommendationReason Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
