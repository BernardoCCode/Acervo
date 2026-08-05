using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Reading;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/reading")]
public sealed class ReadingController : ControllerBase
{
    private readonly ReadingService _reading;

    public ReadingController(ReadingService reading)
    {
        _reading = reading;
    }

    [HttpPost("sessions/{articleId:guid}")]
    public async Task<IActionResult> OpenSession(Guid articleId, CancellationToken cancellationToken)
    {
        var session = await _reading.OpenSessionAsync(articleId, cancellationToken);
        return Ok(session);
    }

    [HttpPut("sessions/progress")]
    public async Task<IActionResult> UpdateProgress(
        [FromBody] UpdateReadingProgressRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _reading.UpdateProgressAsync(request, cancellationToken);
        return Ok(session);
    }

    [HttpPost("sessions/highlights")]
    public async Task<IActionResult> AddHighlight(
        [FromBody] AddHighlightRequest request,
        CancellationToken cancellationToken)
    {
        var session = await _reading.AddHighlightAsync(request, cancellationToken);
        return Ok(session);
    }

    [HttpDelete("sessions/{sessionId:guid}/highlights/{highlightId:guid}")]
    public async Task<IActionResult> RemoveHighlight(
        Guid sessionId,
        Guid highlightId,
        CancellationToken cancellationToken)
    {
        var session = await _reading.RemoveHighlightAsync(
            sessionId,
            highlightId,
            cancellationToken);
        return Ok(session);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var preferences = await _reading.GetPreferencesAsync(cancellationToken);
        return Ok(preferences);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateReaderPreferencesRequest request,
        CancellationToken cancellationToken)
    {
        var preferences = await _reading.UpdatePreferencesAsync(request, cancellationToken);
        return Ok(preferences);
    }
}
