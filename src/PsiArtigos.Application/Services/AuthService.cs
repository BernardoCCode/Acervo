using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Auth;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHashPort _passwords;
    private readonly IAccessTokenPort _tokens;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository users,
        IPasswordHashPort passwords,
        IAccessTokenPort tokens,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.Password);
        if (await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken) is not null)
            throw new ConflictException("Já existe uma conta com este e-mail.");

        var user = User.Create(
            request.Email,
            _passwords.Hash(request.Password),
            UserProfile.Create(request.DisplayName, "pt"));
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateResponse(user, request.RememberMe);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(),
            cancellationToken);
        if (user is null || !_passwords.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedAppException("E-mail ou senha incorretos.");

        return CreateResponse(user, request.RememberMe);
    }

    /// <summary>
    /// Shared demo visitor — ensures a fixed guest user exists and issues a short-lived JWT.
    /// </summary>
    public async Task<AuthResponse> GuestAsync(CancellationToken cancellationToken = default)
    {
        const string guestEmail = "guest@acervo.local";

        var user = await _users.GetByEmailAsync(guestEmail, cancellationToken);
        if (user is null)
        {
            user = User.Create(
                guestEmail,
                _passwords.Hash(Guid.NewGuid().ToString("N") + "a1"),
                UserProfile.Create("Visitante", "pt"));
            await _users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return CreateResponse(user, rememberMe: false);
    }

    public async Task<AuthUserDto> MeAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAppException();
        return ToDto(user);
    }

    private AuthResponse CreateResponse(User user, bool rememberMe)
    {
        var (token, expiresAtUtc) = _tokens.Create(user, rememberMe);
        return new AuthResponse(token, expiresAtUtc, ToDto(user));
    }

    private static AuthUserDto ToDto(User user)
        => new(
            user.Id.Value,
            user.Email,
            user.Profile.DisplayName,
            user.Profile.PreferredLanguage ?? "pt",
            user.Profile.Interests.Select(x => x.Value).ToList());

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("A senha deve ter pelo menos 8 caracteres.");
        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new ArgumentException("A senha deve conter letras e números.");
    }
}
