namespace PsiArtigos.Domain.ValueObjects;

public readonly record struct ReadingSessionId(Guid Value)
{
    public static ReadingSessionId New() => new(Guid.NewGuid());

    public static ReadingSessionId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ReadingSessionId cannot be empty.", nameof(value));

        return new ReadingSessionId(value);
    }

    public override string ToString() => Value.ToString();
}
