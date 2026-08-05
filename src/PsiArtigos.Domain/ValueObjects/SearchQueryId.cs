namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct SearchQueryId(Guid Value)
{
    public static SearchQueryId New() => new(Guid.NewGuid());

    public static SearchQueryId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SearchQueryId cannot be empty.", nameof(value));

        return new SearchQueryId(value);
    }

    public override string ToString() => Value.ToString();
}
