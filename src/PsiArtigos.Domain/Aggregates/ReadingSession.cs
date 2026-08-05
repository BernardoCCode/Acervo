using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class ReadingSession : AggregateRoot<ReadingSessionId>
{
    private readonly List<Highlight> _highlights = [];

    public UserId UserId { get; private set; }
    public ArticleId ArticleId { get; private set; }
    public ReadingProgress Progress { get; private set; } = null!;
    public DateTime LastOpenedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsCompleted { get; private set; }
    public int OpenCount { get; private set; }
    public int ActiveReadingSeconds { get; private set; }

    public IReadOnlyCollection<Highlight> Highlights => _highlights.AsReadOnly();

    private ReadingSession()
    {
    }

    public static ReadingSession Open(
        UserId userId,
        ArticleId articleId,
        DateTime? openedAtUtc = null)
    {
        var now = openedAtUtc ?? DateTime.UtcNow;

        return new ReadingSession
        {
            Id = ReadingSessionId.New(),
            UserId = userId,
            ArticleId = articleId,
            Progress = ReadingProgress.Start(),
            LastOpenedAtUtc = now,
            CreatedAtUtc = now,
            IsCompleted = false,
            OpenCount = 0,
            ActiveReadingSeconds = 0
        };
    }

    public void Touch(DateTime? openedAtUtc = null)
    {
        LastOpenedAtUtc = openedAtUtc ?? DateTime.UtcNow;
        OpenCount++;
    }

    public void UpdateProgress(ReadingProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var percent = Math.Max(Progress.Percent, progress.Percent);
        Progress = ReadingProgress.Create(percent, progress.PageNumber, progress.CharacterOffset);
        IsCompleted = Progress.IsCompleted;
        LastOpenedAtUtc = DateTime.UtcNow;
    }

    public void RecordActiveReading(int seconds)
    {
        if (seconds <= 0)
            return;

        ActiveReadingSeconds += Math.Min(seconds, 300);
        LastOpenedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Progress = ReadingProgress.Create(100, Progress.PageNumber, Progress.CharacterOffset);
        IsCompleted = true;
        LastOpenedAtUtc = DateTime.UtcNow;
    }

    public Highlight AddHighlight(
        TextRange range,
        string quotedText,
        HighlightColor color = HighlightColor.Yellow)
    {
        var highlight = Highlight.Create(range, quotedText, color);
        _highlights.Add(highlight);
        Raise(new HighlightCreated(Id, highlight.Id, UserId, ArticleId, DateTime.UtcNow));
        LastOpenedAtUtc = DateTime.UtcNow;
        return highlight;
    }

    public Annotation AddAnnotation(HighlightId highlightId, string note)
    {
        var highlight = GetHighlight(highlightId);
        var annotation = highlight.AddAnnotation(note);
        LastOpenedAtUtc = DateTime.UtcNow;
        return annotation;
    }

    public void UpdateAnnotation(HighlightId highlightId, AnnotationId annotationId, string note)
    {
        var highlight = GetHighlight(highlightId);
        var annotation = highlight.Annotations.SingleOrDefault(a => a.Id == annotationId)
            ?? throw new DomainException("Annotation was not found.");

        annotation.UpdateNote(note);
        LastOpenedAtUtc = DateTime.UtcNow;
    }

    public bool RemoveHighlight(HighlightId highlightId)
    {
        var removed = _highlights.RemoveAll(h => h.Id == highlightId) > 0;
        if (removed)
            LastOpenedAtUtc = DateTime.UtcNow;

        return removed;
    }

    public bool RemoveAnnotation(HighlightId highlightId, AnnotationId annotationId)
    {
        var highlight = GetHighlight(highlightId);
        var removed = highlight.RemoveAnnotation(annotationId);
        if (removed)
            LastOpenedAtUtc = DateTime.UtcNow;

        return removed;
    }

    public void EnsureOwnedBy(UserId userId)
    {
        if (UserId != userId)
            throw new DomainException("Reading session does not belong to this user.");
    }

    private Highlight GetHighlight(HighlightId highlightId)
        => _highlights.SingleOrDefault(h => h.Id == highlightId)
           ?? throw new DomainException("Highlight was not found.");
}

