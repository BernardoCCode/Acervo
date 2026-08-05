using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Entities;

public sealed class Highlight : Entity<HighlightId>
{
    private readonly List<Annotation> _annotations = [];

    public TextRange Range { get; private set; } = null!;
    public string QuotedText { get; private set; } = null!;
    public HighlightColor Color { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<Annotation> Annotations => _annotations.AsReadOnly();

    private Highlight()
    {
    }

    internal static Highlight Create(
        TextRange range,
        string quotedText,
        HighlightColor color,
        DateTime? createdAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (string.IsNullOrWhiteSpace(quotedText))
            throw new DomainException("Highlighted text is required.");

        if (!Enum.IsDefined(color))
            throw new DomainException("Invalid highlight color.");

        return new Highlight
        {
            Id = HighlightId.New(),
            Range = range,
            QuotedText = quotedText.Trim(),
            Color = color,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        };
    }

    internal Annotation AddAnnotation(string note)
    {
        var annotation = Annotation.Create(Id, note);
        _annotations.Add(annotation);
        return annotation;
    }

    internal bool RemoveAnnotation(AnnotationId annotationId)
        => _annotations.RemoveAll(a => a.Id == annotationId) > 0;

    internal void ChangeColor(HighlightColor color)
    {
        if (!Enum.IsDefined(color))
            throw new DomainException("Invalid highlight color.");

        Color = color;
    }
}
