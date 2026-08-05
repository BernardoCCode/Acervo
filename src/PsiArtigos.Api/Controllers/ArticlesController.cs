using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.Services;
using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/articles")]
public sealed class ArticlesController : ControllerBase
{
    private readonly ArticleService _articles;

    public ArticlesController(ArticleService articles)
    {
        _articles = articles;
    }

    [HttpGet("{articleId:guid}")]
    public async Task<IActionResult> GetById(Guid articleId, CancellationToken cancellationToken)
    {
        var article = await _articles.GetByIdAsync(articleId, cancellationToken);
        return Ok(article);
    }

    [HttpGet("{articleId:guid}/citation")]
    public async Task<IActionResult> ExportCitation(
        Guid articleId,
        [FromQuery] CitationStyle style = CitationStyle.Apa,
        CancellationToken cancellationToken = default)
    {
        var citation = await _articles.ExportCitationAsync(articleId, style, cancellationToken);
        return Ok(new { style, citation });
    }

    [HttpGet("{articleId:guid}/pdf")]
    public async Task<IActionResult> GetPdf(Guid articleId, CancellationToken cancellationToken)
    {
        var pdf = await _articles.GetPdfAsync(articleId, cancellationToken);
        var safeName = (pdf.FileName ?? "article.pdf").Replace("\"", string.Empty);
        Response.Headers.ContentDisposition = $"inline; filename=\"{safeName}\"";
        return File(pdf.Bytes, pdf.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("{articleId:guid}/content")]
    public async Task<IActionResult> GetReadableContent(
        Guid articleId,
        [FromQuery] bool refresh = false,
        CancellationToken cancellationToken = default)
    {
        var content = await _articles.GetReadableContentAsync(
            articleId,
            refresh,
            cancellationToken);
        return Ok(content);
    }
}
