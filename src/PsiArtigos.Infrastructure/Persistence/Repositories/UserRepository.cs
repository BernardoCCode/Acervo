using Microsoft.EntityFrameworkCore;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly PsiArtigosDbContext _dbContext;

    public UserRepository(PsiArtigosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        => _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AddAsync(user, cancellationToken);
}
