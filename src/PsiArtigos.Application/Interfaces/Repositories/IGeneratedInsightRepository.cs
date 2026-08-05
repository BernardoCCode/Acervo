using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IGeneratedInsightRepository
{
    Task<GeneratedInsight?> GetByIdAsync(
        InsightId id,
        CancellationToken cancellationToken = default);

    Task<GeneratedInsight?> GetLatestAsync(
        UserId userId,
        ArticleId articleId,
        InsightType type,
        CancellationToken cancellationToken = default);

    Task AddAsync(GeneratedInsight insight, CancellationToken cancellationToken = default);
}
