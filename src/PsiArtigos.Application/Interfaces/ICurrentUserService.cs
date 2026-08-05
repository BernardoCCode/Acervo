using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Interfaces;

public interface ICurrentUserService
{
    UserId? UserId { get; }
    bool IsAuthenticated { get; }

    UserId GetRequiredUserId();
}
