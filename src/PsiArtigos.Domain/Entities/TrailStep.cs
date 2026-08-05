using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Entities;

public sealed class TrailStep : Entity<TrailStepId>
{
    public int Order { get; private set; }
    public string Title { get; private set; } = null!;
    public DifficultyLevel Difficulty { get; private set; }
    public ArticleId? ArticleId { get; private set; }
    public string? Rationale { get; private set; }

    private TrailStep()
    {
    }

    internal static TrailStep Create(
        int order,
        string title,
        DifficultyLevel difficulty,
        ArticleId? articleId = null,
        string? rationale = null)
    {
        if (order < 1)
            throw new DomainException("Trail step order must be greater than zero.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Trail step title is required.");

        if (!Enum.IsDefined(difficulty))
            throw new DomainException("Invalid difficulty level.");

        return new TrailStep
        {
            Id = TrailStepId.New(),
            Order = order,
            Title = title.Trim(),
            Difficulty = difficulty,
            ArticleId = articleId,
            Rationale = string.IsNullOrWhiteSpace(rationale) ? null : rationale.Trim()
        };
    }

    internal void AssignArticle(ArticleId articleId)
    {
        ArticleId = articleId;
    }

    internal void Reorder(int order)
    {
        if (order < 1)
            throw new DomainException("Trail step order must be greater than zero.");

        Order = order;
    }

    public bool HasArticle => ArticleId is not null;
}