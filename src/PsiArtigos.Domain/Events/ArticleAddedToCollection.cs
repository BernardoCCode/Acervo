using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record ArticleAddedToCollection(
    CollectionId CollectionId,
    UserId UserId,
    ArticleId ArticleId,
    DateTime OccurredOnUtc) : IDomainEvent;