using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record HighlightCreated(
    ReadingSessionId SessionId,
    HighlightId HighlightId,
    UserId UserId,
    ArticleId ArticleId,
    DateTime OccurredOnUtc) : IDomainEvent;
