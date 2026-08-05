using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Articles;
using PsiArtigos.Application.DTOs.Library;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class LibraryService
{
    private readonly IFavoriteRepository _favorites;
    private readonly ICollectionRepository _collections;
    private readonly IArticleRepository _articles;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public LibraryService(
        IFavoriteRepository favorites,
        ICollectionRepository collections,
        IArticleRepository articles,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _favorites = favorites;
        _collections = collections;
        _articles = articles;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<FavoriteDto> FavoriteAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var typedArticleId = ArticleId.From(articleId);

        var article = await _articles.GetByIdAsync(typedArticleId, cancellationToken);
        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        var existing = await _favorites.GetAsync(userId, typedArticleId, cancellationToken);
        if (existing is not null)
        {
            return new FavoriteDto(
                existing.Id.Value,
                existing.ArticleId.Value,
                existing.CreatedAtUtc);
        }

        var favorite = Favorite.Create(userId, typedArticleId);
        await _favorites.AddAsync(favorite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FavoriteDto(
            favorite.Id.Value,
            favorite.ArticleId.Value,
            favorite.CreatedAtUtc);
    }

    public async Task UnfavoriteAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var favorite = await _favorites.GetAsync(
            userId,
            ArticleId.From(articleId),
            cancellationToken);

        if (favorite is null)
            return;

        _favorites.Remove(favorite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteDto>> ListFavoritesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var favorites = await _favorites.ListByUserAsync(userId, cancellationToken);
        var ordered = favorites.OrderByDescending(f => f.CreatedAtUtc).ToList();
        var articles = await _articles.GetByIdsAsync(
            ordered.Select(f => f.ArticleId),
            cancellationToken);
        var articlesById = articles.ToDictionary(a => a.Id);

        return ordered
            .Select(f => new FavoriteDto(
                f.Id.Value,
                f.ArticleId.Value,
                f.CreatedAtUtc,
                articlesById.TryGetValue(f.ArticleId, out var article) ? article.ToDto() : null))
            .ToList();
    }

    public async Task<CollectionDto> CreateCollectionAsync(
        CreateCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (await _collections.ExistsWithNameAsync(userId, request.Name, cancellationToken))
            throw new ConflictException($"Collection '{request.Name}' already exists.");

        var collection = Collection.Create(userId, request.Name, request.Description);
        await _collections.AddAsync(collection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CollectionDto(
            collection.Id.Value,
            collection.Name,
            collection.Description,
            collection.Items.Count,
            collection.CreatedAtUtc);
    }

    public async Task AddArticleToCollectionAsync(
        Guid collectionId,
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var collection = await _collections.GetByIdAsync(
            CollectionId.From(collectionId),
            cancellationToken);

        if (collection is null)
            throw NotFoundException.For<Collection>(collectionId);

        collection.EnsureOwnedBy(userId);

        var article = await _articles.GetByIdAsync(
            ArticleId.From(articleId),
            cancellationToken);

        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        collection.AddArticle(article.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveArticleFromCollectionAsync(
        Guid collectionId,
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var collection = await _collections.GetByIdAsync(
            CollectionId.From(collectionId),
            cancellationToken);

        if (collection is null)
            throw NotFoundException.For<Collection>(collectionId);

        collection.EnsureOwnedBy(userId);
        collection.RemoveArticle(ArticleId.From(articleId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CollectionDetailDto> GetCollectionAsync(
        Guid collectionId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var collection = await _collections.GetByIdAsync(
            CollectionId.From(collectionId),
            cancellationToken);

        if (collection is null)
            throw NotFoundException.For<Collection>(collectionId);

        collection.EnsureOwnedBy(userId);

        var orderedItems = collection.Items
            .OrderByDescending(i => i.AddedAtUtc)
            .ToList();

        var articles = await _articles.GetByIdsAsync(
            orderedItems.Select(i => i.ArticleId),
            cancellationToken);
        var articlesById = articles.ToDictionary(a => a.Id);

        var articleDtos = orderedItems
            .Select(i => articlesById.TryGetValue(i.ArticleId, out var article) ? article.ToDto() : null)
            .OfType<ArticleDto>()
            .ToList();

        return new CollectionDetailDto(
            collection.Id.Value,
            collection.Name,
            collection.Description,
            collection.Items.Count,
            collection.CreatedAtUtc,
            articleDtos);
    }

    public async Task<IReadOnlyList<CollectionDto>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var collections = await _collections.ListByUserAsync(userId, cancellationToken);

        return collections
            .OrderBy(c => c.Name)
            .Select(c => new CollectionDto(
                c.Id.Value,
                c.Name,
                c.Description,
                c.Items.Count,
                c.CreatedAtUtc))
            .ToList();
    }
}
