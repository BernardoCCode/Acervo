using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.DTOs.Learning;

public sealed record CreateLearningTrailRequest(string Prompt);

public sealed record LearningTrailDto(
    Guid Id,
    string Prompt,
    string Topic,
    TrailStatus Status,
    string? FailureReason,
    IReadOnlyList<TrailStepDto> Steps,
    DateTime CreatedAtUtc);

public sealed record TrailStepDto(
    Guid Id,
    int Order,
    string Title,
    DifficultyLevel Difficulty,
    Guid? ArticleId,
    string? ArticleTitle,
    string? Rationale);
