using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record LearningTrailCreated(
    LearningTrailId TrailId,
    UserId UserId,
    string Topic,
    DateTime OccurredOnUtc) : IDomainEvent;