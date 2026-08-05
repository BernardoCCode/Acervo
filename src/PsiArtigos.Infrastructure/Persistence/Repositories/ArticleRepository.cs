using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class ArticleRepository : IArticleRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public ArticleRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Article?> GetByIdAsync(ArticleId id, CancellationToken cancellationToken = default)
        => _dbContext.Articles.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<Article?> GetByDoiAsync(string doi, CancellationToken cancellationToken = default)
    {
        Doi doiValue;
        try
        {
            doiValue = Doi.Create(doi);
        }
        catch
        {
            return Task.FromResult<Article?>(null);
        }

        return _dbContext.Articles
            .FirstOrDefaultAsync(
                a => a.Publication.Doi != null && a.Publication.Doi == doiValue,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Article>> GetByDoisAsync(
        IEnumerable<string> dois,
        CancellationToken cancellationToken = default)
    {
        var doiValues = new List<Doi>();
        foreach (var doi in dois.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                doiValues.Add(Doi.Create(doi));
            }
            catch
            {
                // Skip malformed DOIs — same behavior as GetByDoiAsync.
            }
        }

        if (doiValues.Count == 0)
            return [];

        return await _dbContext.Articles
            .Where(a => a.Publication.Doi != null && doiValues.Contains(a.Publication.Doi))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Article>> GetByIdsAsync(
        IEnumerable<ArticleId> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await _dbContext.Articles
            .Where(a => idList.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Article article, CancellationToken cancellationToken = default)
        => await _dbContext.Articles.AddAsync(article, cancellationToken);
}
