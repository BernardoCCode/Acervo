using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record InsightGenerated(
    InsightId InsightId,
    UserId UserId,
    ArticleId ArticleId,
    InsightType Type,
    DateTime OccurredOnUtc) : IDomainEvent;
