using PsiArtigos.Domain.Aggregates;

namespace PsiArtigos.Application.Interfaces;

public interface IPasswordHashPort
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public interface IAccessTokenPort
{
    (string Token, DateTime ExpiresAtUtc) Create(User user, bool rememberMe);
}
