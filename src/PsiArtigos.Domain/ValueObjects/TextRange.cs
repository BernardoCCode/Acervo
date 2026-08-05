using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class TextRange : ValueObject
{
    public int StartOffset { get; private set; }
    public int EndOffset { get; private set; }
    public int? PageNumber { get; private set; }

    private TextRange()
    {
    }

    private TextRange(int startOffset, int endOffset, int? pageNumber)
    {
        StartOffset = startOffset;
        EndOffset = endOffset;
        PageNumber = pageNumber;
    }

    public static TextRange Create(int startOffset, int endOffset, int? pageNumber = null)
    {
        if (startOffset < 0)
            throw new DomainException("Text range start offset cannot be negative.");

        if (endOffset <= startOffset)
            throw new DomainException("Text range end offset must be greater than start offset.");

        if (pageNumber is <= 0)
            throw new DomainException("Page number must be greater than zero when provided.");

        return new TextRange(startOffset, endOffset, pageNumber);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartOffset;
        yield return EndOffset;
        yield return PageNumber;
    }
}
