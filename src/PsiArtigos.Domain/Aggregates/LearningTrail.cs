using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class LearningTrail : AggregateRoot<LearningTrailId>
{
    private readonly List<TrailStep> _steps = [];

    public UserId UserId { get; private set; }
    public string Prompt { get; private set; } = null!;
    public string Topic { get; private set; } = null!;
    public TrailStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<TrailStep> Steps => _steps.OrderBy(s => s.Order).ToList().AsReadOnly();

    private LearningTrail()
    {
    }

    public static LearningTrail Create(
        UserId userId,
        string prompt,
        string topic,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new DomainException("Learning trail prompt is required.");

        if (string.IsNullOrWhiteSpace(topic))
            throw new DomainException("Learning trail topic is required.");

        var now = createdAtUtc ?? DateTime.UtcNow;

        var trail = new LearningTrail
        {
            Id = LearningTrailId.New(),
            UserId = userId,
            Prompt = prompt.Trim(),
            Topic = topic.Trim(),
            Status = TrailStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        trail.Raise(new LearningTrailCreated(trail.Id, userId, trail.Topic, now));
        return trail;
    }

    public TrailStep AddStep(
        string title,
        DifficultyLevel difficulty,
        ArticleId? articleId = null,
        string? rationale = null)
    {
        EnsureNotFailed();

        var order = _steps.Count == 0 ? 1 : _steps.Max(s => s.Order) + 1;
        var step = TrailStep.Create(order, title, difficulty, articleId, rationale);
        _steps.Add(step);

        if (Status == TrailStatus.Ready)
            Status = TrailStatus.Draft;

        Touch();
        return step;
    }

    public void AssignArticleToStep(TrailStepId stepId, ArticleId articleId)
    {
        EnsureNotFailed();

        var step = _steps.SingleOrDefault(s => s.Id == stepId)
            ?? throw new DomainException("Trail step was not found.");

        step.AssignArticle(articleId);

        if (Status == TrailStatus.Ready)
            Status = TrailStatus.Draft;

        Touch();
    }

    public void MarkReady()
    {
        if (_steps.Count == 0)
            throw new DomainException("A learning trail needs at least one step to be ready.");

        if (_steps.Any(s => !s.HasArticle))
            throw new DomainException("All trail steps must have an article before the trail is ready.");

        EnsureSequentialOrders();

        Status = TrailStatus.Ready;
        FailureReason = null;

        var now = DateTime.UtcNow;
        UpdatedAtUtc = now;
        Raise(new LearningTrailReady(Id, UserId, _steps.Count, now));
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason is required.");

        Status = TrailStatus.Failed;
        FailureReason = reason.Trim();
        Touch();
    }

    public void EnsureOwnedBy(UserId userId)
    {
        if (UserId != userId)
            throw new DomainException("Learning trail does not belong to this user.");
    }

    private void EnsureNotFailed()
    {
        if (Status == TrailStatus.Failed)
            throw new DomainException("A failed learning trail cannot be modified. Create a new trail instead.");
    }

    private void EnsureSequentialOrders()
    {
        var orders = _steps.Select(s => s.Order).OrderBy(o => o).ToList();

        for (var i = 0; i < orders.Count; i++)
        {
            if (orders[i] != i + 1)
                throw new DomainException("Trail steps must have sequential order starting at 1.");
        }
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}