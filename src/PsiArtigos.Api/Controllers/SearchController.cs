using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Search;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly SearchService _search;

    public SearchController(SearchService search)
    {
        _search = search;
    }

    [HttpPost]
    public async Task<IActionResult> Search(
        [FromBody] SearchArticlesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _search.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var history = await _search.ListHistoryAsync(take, cancellationToken);
        return Ok(history);
    }
}
