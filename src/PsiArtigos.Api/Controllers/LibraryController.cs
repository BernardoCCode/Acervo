using Microsoft.AspNetCore.Mvc;
using PsiArtigos.Application.DTOs.Library;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Api.Controllers;

[ApiController]
[Route("api/library")]
public sealed class LibraryController : ControllerBase
{
    private readonly LibraryService _library;

    public LibraryController(LibraryService library)
    {
        _library = library;
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> ListFavorites(CancellationToken cancellationToken)
    {
        var favorites = await _library.ListFavoritesAsync(cancellationToken);
        return Ok(favorites);
    }

    [HttpPost("favorites/{articleId:guid}")]
    public async Task<IActionResult> Favorite(Guid articleId, CancellationToken cancellationToken)
    {
        var favorite = await _library.FavoriteAsync(articleId, cancellationToken);
        return Ok(favorite);
    }

    [HttpDelete("favorites/{articleId:guid}")]
    public async Task<IActionResult> Unfavorite(Guid articleId, CancellationToken cancellationToken)
    {
        await _library.UnfavoriteAsync(articleId, cancellationToken);
        return NoContent();
    }

    [HttpGet("collections")]
    public async Task<IActionResult> ListCollections(CancellationToken cancellationToken)
    {
        var collections = await _library.ListCollectionsAsync(cancellationToken);
        return Ok(collections);
    }

    [HttpPost("collections")]
    public async Task<IActionResult> CreateCollection(
        [FromBody] CreateCollectionRequest request,
        CancellationToken cancellationToken)
    {
        var collection = await _library.CreateCollectionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ListCollections), collection);
    }

    [HttpGet("collections/{collectionId:guid}")]
    public async Task<IActionResult> GetCollection(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        var collection = await _library.GetCollectionAsync(collectionId, cancellationToken);
        return Ok(collection);
    }

    [HttpPost("collections/{collectionId:guid}/articles/{articleId:guid}")]
    public async Task<IActionResult> AddArticleToCollection(
        Guid collectionId,
        Guid articleId,
        CancellationToken cancellationToken)
    {
        await _library.AddArticleToCollectionAsync(collectionId, articleId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("collections/{collectionId:guid}/articles/{articleId:guid}")]
    public async Task<IActionResult> RemoveArticleFromCollection(
        Guid collectionId,
        Guid articleId,
        CancellationToken cancellationToken)
    {
        await _library.RemoveArticleFromCollectionAsync(collectionId, articleId, cancellationToken);
        return NoContent();
    }
}
