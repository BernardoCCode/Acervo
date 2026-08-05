using PsiArtigos.Application.DTOs.Learning;
using PsiArtigos.Domain.Aggregates;

namespace PsiArtigos.Application.Mapping;

public static class LearningMapping
{
    public static LearningTrailDto ToDto(
        this LearningTrail trail,
        IReadOnlyDictionary<Guid, string>? articleTitles = null)
    {
        return new LearningTrailDto(
            trail.Id.Value,
            trail.Prompt,
            trail.Topic,
            trail.Status,
            trail.FailureReason,
            trail.Steps.Select(step => new TrailStepDto(
                step.Id.Value,
                step.Order,
                step.Title,
                step.Difficulty,
                step.ArticleId?.Value,
                step.ArticleId is not null
                    && articleTitles?.TryGetValue(step.ArticleId.Value.Value, out var articleTitle) == true
                        ? articleTitle
                        : null,
                step.Rationale)).ToList(),
            trail.CreatedAtUtc);
    }
}
