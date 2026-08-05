using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record ArticleFavorited(
    FavoriteId FavoriteId,
    UserId UserId,
    ArticleId ArticleId,
    DateTime OccurredOnUtc) : IDomainEvent;