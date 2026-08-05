using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class TopicTag : ValueObject
{
    public string Value { get; private set; } = null!;

    private TopicTag()
    {
    }

    private TopicTag(string value) => Value = value;

    public static TopicTag Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Topic tag is required.");

        return new TopicTag(value.Trim());
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
