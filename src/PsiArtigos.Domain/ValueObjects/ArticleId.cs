namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct ArticleId(Guid Value)
{
    public static ArticleId New() => new(Guid.NewGuid());

    public static ArticleId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ArticleId cannot be empty.", nameof(value));

        return new ArticleId(value);
    }

    public override string ToString() => Value.ToString();
}
