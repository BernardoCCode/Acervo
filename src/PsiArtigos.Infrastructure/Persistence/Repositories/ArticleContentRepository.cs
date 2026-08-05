using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class ArticleContentRepository : IArticleContentRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public ArticleContentRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ArticleContent?> GetByArticleIdAsync(
        ArticleId articleId,
        CancellationToken cancellationToken = default)
        => _dbContext.ArticleContents.FirstOrDefaultAsync(c => c.Id == articleId, cancellationToken);

    public async Task AddAsync(ArticleContent content, CancellationToken cancellationToken = default)
        => await _dbContext.ArticleContents.AddAsync(content, cancellationToken);
}
