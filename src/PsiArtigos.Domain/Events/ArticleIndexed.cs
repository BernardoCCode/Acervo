using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record ArticleIndexed(ArticleId ArticleId, DateTime OccurredOnUtc) : IDomainEvent;