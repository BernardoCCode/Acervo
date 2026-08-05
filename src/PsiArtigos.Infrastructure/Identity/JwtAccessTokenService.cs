using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.Identity;

public sealed class JwtAccessTokenService : IAccessTokenPort
{
    private readonly JwtOptions _options;

    public JwtAccessTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) Create(User user, bool rememberMe)
    {
        var now = DateTime.UtcNow;
        var expires = rememberMe
            ? now.AddDays(_options.RememberMeDays)
            : now.AddHours(_options.ExpirationHours);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Profile.DisplayName ?? user.Email)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            expires,
            credentials);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
