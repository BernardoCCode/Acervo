using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IArticleContentRepository
{
    Task<ArticleContent?> GetByArticleIdAsync(
        ArticleId articleId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ArticleContent content, CancellationToken cancellationToken = default);
}
