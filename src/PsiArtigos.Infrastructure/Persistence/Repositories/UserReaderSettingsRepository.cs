using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class UserReaderSettingsRepository : IUserReaderSettingsRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public UserReaderSettingsRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserReaderSettings?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
        => _dbContext.UserReaderSettings.FirstOrDefaultAsync(s => s.Id == userId, cancellationToken);

    public async Task AddAsync(UserReaderSettings settings, CancellationToken cancellationToken = default)
        => await _dbContext.UserReaderSettings.AddAsync(settings, cancellationToken);
}
