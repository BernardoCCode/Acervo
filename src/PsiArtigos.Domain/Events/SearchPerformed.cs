using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Events;

public sealed record SearchPerformed(
    SearchQueryId SearchQueryId,
    UserId? UserId,
    string RawText,
    int ResultCount,
    DateTime OccurredOnUtc) : IDomainEvent;
