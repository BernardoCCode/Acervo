namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct FavoriteId(Guid Value)
{
    public static FavoriteId New() => new(Guid.NewGuid());

    public static FavoriteId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FavoriteId cannot be empty.", nameof(value));

        return new FavoriteId(value);
    }

    public override string ToString() => Value.ToString();
}
