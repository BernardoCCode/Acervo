namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct InsightId(Guid Value)
{
    public static InsightId New() => new(Guid.NewGuid());

    public static InsightId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("InsightId cannot be empty.", nameof(value));

        return new InsightId(value);
    }

    public override string ToString() => Value.ToString();
}
