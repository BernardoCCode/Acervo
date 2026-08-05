using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class User : AggregateRoot<UserId>
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserProfile Profile { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private User()
    {
    }

    public static User Create(
        string email,
        string passwordHash,
        UserProfile? profile = null,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!normalizedEmail.Contains('@') || normalizedEmail.StartsWith('@') || normalizedEmail.EndsWith('@'))
            throw new DomainException("Email format is invalid.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        var now = createdAtUtc ?? DateTime.UtcNow;

        return new User
        {
            Id = UserId.New(),
            Email = normalizedEmail,
            PasswordHash = passwordHash.Trim(),
            Profile = profile ?? UserProfile.Create(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void UpdateProfile(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Profile = profile;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!normalizedEmail.Contains('@') || normalizedEmail.StartsWith('@') || normalizedEmail.EndsWith('@'))
            throw new DomainException("Email format is invalid.");

        Email = normalizedEmail;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = passwordHash.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}