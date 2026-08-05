using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Recommendations;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public sealed class RecommendationsController : ControllerBase
{
    private readonly RecommendationService _recommendations;

    public RecommendationsController(RecommendationService recommendations)
    {
        _recommendations = recommendations;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendations.ListAsync(take, cancellationToken);
        return Ok(recommendations);
    }

    [HttpDelete("{recommendationId:guid}")]
    public async Task<IActionResult> Dismiss(Guid recommendationId, CancellationToken cancellationToken)
    {
        await _recommendations.DismissAsync(recommendationId, cancellationToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var generated = await _recommendations.RefreshAsync(cancellationToken);
        return Ok(new RecommendationRefreshDto(generated));
    }
}
