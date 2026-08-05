using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Entities;

public sealed class Annotation : Entity<AnnotationId>
{
    public HighlightId HighlightId { get; private set; }
    public string Note { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Annotation()
    {
    }

    internal static Annotation Create(HighlightId highlightId, string note, DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new DomainException("Annotation note is required.");

        var now = createdAtUtc ?? DateTime.UtcNow;

        return new Annotation
        {
            Id = AnnotationId.New(),
            HighlightId = highlightId,
            Note = note.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    internal void UpdateNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new DomainException("Annotation note is required.");

        Note = note.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
