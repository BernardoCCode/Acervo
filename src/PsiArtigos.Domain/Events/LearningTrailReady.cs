using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record LearningTrailReady(
    LearningTrailId TrailId,
    UserId UserId,
    int StepCount,
    DateTime OccurredOnUtc) : IDomainEvent;