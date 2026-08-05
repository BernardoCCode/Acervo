namespace PsiArtigos.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
