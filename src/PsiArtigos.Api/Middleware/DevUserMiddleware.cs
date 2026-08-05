using System.Security.Claims;

namespace PsiArtigos.Api.Middleware;

/// <summary>
/// Development helper: authenticates requests using X-User-Id header (or a default demo user).
/// </summary>
public sealed class DevUserMiddleware
{
    public const string DefaultUserId = "11111111-1111-1111-1111-111111111111";
    public const string UserIdHeader = "X-User-Id";

    private readonly RequestDelegate _next;

    public DevUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            var rawUserId = context.Request.Headers[UserIdHeader].FirstOrDefault();
            if (!Guid.TryParse(rawUserId, out var userId))
                userId = Guid.Parse(DefaultUserId);

            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("sub", userId.ToString())
            ],
            authenticationType: "DevUser");

            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
