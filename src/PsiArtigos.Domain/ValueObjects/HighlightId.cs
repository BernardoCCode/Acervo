namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct HighlightId(Guid Value)
{
    public static HighlightId New() => new(Guid.NewGuid());

    public static HighlightId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("HighlightId cannot be empty.", nameof(value));

        return new HighlightId(value);
    }

    public override string ToString() => Value.ToString();
}
