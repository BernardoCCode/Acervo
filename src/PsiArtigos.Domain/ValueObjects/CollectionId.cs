namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct CollectionId(Guid Value)
{
    public static CollectionId New() => new(Guid.NewGuid());

    public static CollectionId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CollectionId cannot be empty.", nameof(value));

        return new CollectionId(value);
    }

    public override string ToString() => Value.ToString();
}
