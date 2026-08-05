using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Insights;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/insights")]
public sealed class InsightsController : ControllerBase
{
    private readonly InsightService _insights;

    public InsightsController(InsightService insights)
    {
        _insights = insights;
    }

    [HttpPost]
    public async Task<IActionResult> Generate(
        [FromBody] GenerateInsightRequest request,
        CancellationToken cancellationToken)
    {
        var insight = await _insights.GenerateAsync(request, cancellationToken);
        return Ok(insight);
    }
}
