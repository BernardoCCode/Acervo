using PsiArtigos.Application.Interfaces;

namespace PsiArtigos.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly PsiArtigosDbContext _dbContext;

    public UnitOfWork(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
