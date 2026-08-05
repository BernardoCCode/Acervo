using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Auth;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
        => Ok(await _auth.RegisterAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
        => Ok(await _auth.LoginAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("guest")]
    public async Task<IActionResult> Guest(CancellationToken cancellationToken)
        => Ok(await _auth.GuestAsync(cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
        => Ok(await _auth.MeAsync(cancellationToken));
}
