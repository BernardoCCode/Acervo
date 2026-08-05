namespace PsiArtigos.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? DisplayName = null,
    bool RememberMe = false);

public sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe = false);

public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string PreferredLanguage,
    IReadOnlyList<string> Interests);

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthUserDto User);
