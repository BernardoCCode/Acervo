using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class ReadingProgress : ValueObject
{
    public double Percent { get; private set; }
    public int? PageNumber { get; private set; }
    public int? CharacterOffset { get; private set; }

    private ReadingProgress()
    {
    }

    private ReadingProgress(double percent, int? pageNumber, int? characterOffset)
    {
        Percent = percent;
        PageNumber = pageNumber;
        CharacterOffset = characterOffset;
    }

    public static ReadingProgress Create(double percent, int? pageNumber = null, int? characterOffset = null)
    {
        if (percent is < 0 or > 100)
            throw new DomainException("Reading progress percent must be between 0 and 100.");

        if (pageNumber is <= 0)
            throw new DomainException("Page number must be greater than zero when provided.");

        if (characterOffset is < 0)
            throw new DomainException("Character offset cannot be negative.");

        return new ReadingProgress(Math.Round(percent, 2), pageNumber, characterOffset);
    }

    public static ReadingProgress Start() => new(0, null, 0);

    public bool IsCompleted => Percent >= 100;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Percent;
        yield return PageNumber;
        yield return CharacterOffset;
    }
}
