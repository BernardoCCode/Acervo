namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct RecommendationId(Guid Value)
{
    public static RecommendationId New() => new(Guid.NewGuid());

    public static RecommendationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RecommendationId cannot be empty.", nameof(value));

        return new RecommendationId(value);
    }

    public override string ToString() => Value.ToString();
}
