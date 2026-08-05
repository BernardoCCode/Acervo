using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/features")]
public sealed class FeaturesController : ControllerBase
{
    private readonly AiOptions _ai;

    public FeaturesController(IOptions<AiOptions> ai)
    {
        _ai = ai.Value;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
        => Ok(new FeaturesResponse(
            AiEnabled: !string.IsNullOrWhiteSpace(_ai.ApiKey)));
}

public sealed record FeaturesResponse(bool AiEnabled);
