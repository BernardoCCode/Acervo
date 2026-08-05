using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface IUserReaderSettingsRepository
{
    Task<UserReaderSettings?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(UserReaderSettings settings, CancellationToken cancellationToken = default);
}
