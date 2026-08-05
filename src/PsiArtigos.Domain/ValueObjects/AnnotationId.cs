namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct AnnotationId(Guid Value)
{
    public static AnnotationId New() => new(Guid.NewGuid());

    public static AnnotationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AnnotationId cannot be empty.", nameof(value));

        return new AnnotationId(value);
    }

    public override string ToString() => Value.ToString();
}
