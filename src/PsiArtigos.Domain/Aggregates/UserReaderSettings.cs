using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class UserReaderSettings : AggregateRoot<UserId>
{
    public ReaderPreferences Preferences { get; private set; } = null!;
    public DateTime UpdatedAtUtc { get; private set; }

    private UserReaderSettings()
    {
    }

    public static UserReaderSettings Create(UserId userId, ReaderPreferences? preferences = null)
    {
        return new UserReaderSettings
        {
            Id = userId,
            Preferences = preferences ?? ReaderPreferences.Default(),
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdatePreferences(ReaderPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        Preferences = preferences;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void EnsureOwnedBy(UserId userId)
    {
        if (Id != userId)
            throw new DomainException("Reader settings do not belong to this user.");
    }
}
