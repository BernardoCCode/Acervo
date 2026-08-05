using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.Interfaces;

public interface IAiLearningPort
{
    Task<LearningTrailPlan> PlanTrailAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}

public sealed record LearningTrailPlan(
    string Topic,
    IReadOnlyList<LearningTrailStepPlan> Steps);

public sealed record LearningTrailStepPlan(
    string Title,
    DifficultyLevel Difficulty,
    string SearchQuery,
    string? Rationale);
