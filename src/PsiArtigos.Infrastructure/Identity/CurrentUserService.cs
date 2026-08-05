using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId? UserId
    {
        get
        {
            var raw = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(raw, out var id)
                ? Domain.ValueObjects.UserId.From(id)
                : null;
        }
    }

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
           && UserId is not null;

    public UserId GetRequiredUserId()
        => UserId ?? throw new UnauthorizedAppException();
}
