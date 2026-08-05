using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Learning;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/learning-trails")]
public sealed class LearningTrailsController : ControllerBase
{
    private readonly LearningTrailService _learningTrails;

    public LearningTrailsController(LearningTrailService learningTrails)
    {
        _learningTrails = learningTrails;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var trails = await _learningTrails.ListAsync(cancellationToken);
        return Ok(trails);
    }

    [HttpGet("{trailId:guid}")]
    public async Task<IActionResult> GetById(Guid trailId, CancellationToken cancellationToken)
    {
        var trail = await _learningTrails.GetByIdAsync(trailId, cancellationToken);
        return Ok(trail);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLearningTrailRequest request,
        CancellationToken cancellationToken)
    {
        var trail = await _learningTrails.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { trailId = trail.Id }, trail);
    }
}
