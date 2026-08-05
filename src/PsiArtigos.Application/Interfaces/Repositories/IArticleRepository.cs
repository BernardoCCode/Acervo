using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(ArticleId id, CancellationToken cancellationToken = default);

    Task<Article?> GetByDoiAsync(string doi, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Article>> GetByDoisAsync(
        IEnumerable<string> dois,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Article>> GetByIdsAsync(
        IEnumerable<ArticleId> ids,
        CancellationToken cancellationToken = default);

    Task AddAsync(Article article, CancellationToken cancellationToken = default);
}
