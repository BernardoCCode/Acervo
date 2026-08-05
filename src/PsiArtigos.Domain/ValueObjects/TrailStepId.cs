namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct TrailStepId(Guid Value)
{
    public static TrailStepId New() => new(Guid.NewGuid());

    public static TrailStepId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TrailStepId cannot be empty.", nameof(value));

        return new TrailStepId(value);
    }

    public override string ToString() => Value.ToString();
}
