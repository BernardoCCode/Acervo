using PsiArtigos.Application.DTOs.Articles;

namespace PsiArtigos.Application.DTOs.Library;

public sealed record FavoriteDto(
    Guid FavoriteId,
    Guid ArticleId,
    DateTime CreatedAtUtc,
    ArticleDto? Article = null);

public sealed record CollectionDto(
    Guid Id,
    string Name,
    string? Description,
    int ItemCount,
    DateTime CreatedAtUtc);

public sealed record CollectionDetailDto(
    Guid Id,
    string Name,
    string? Description,
    int ItemCount,
    DateTime CreatedAtUtc,
    IReadOnlyList<ArticleDto> Articles);

public sealed record CreateCollectionRequest(
    string Name,
    string? Description = null);

public sealed record AddArticleToCollectionRequest(
    Guid CollectionId,
    Guid ArticleId);
