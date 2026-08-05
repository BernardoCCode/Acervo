namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct CitationLinkId(Guid Value)
{
    public static CitationLinkId New() => new(Guid.NewGuid());

    public static CitationLinkId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CitationLinkId cannot be empty.", nameof(value));

        return new CitationLinkId(value);
    }

    public override string ToString() => Value.ToString();
}
