namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct LearningTrailId(Guid Value)
{
    public static LearningTrailId New() => new(Guid.NewGuid());

    public static LearningTrailId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LearningTrailId cannot be empty.", nameof(value));

        return new LearningTrailId(value);
    }

    public override string ToString() => Value.ToString();
}
