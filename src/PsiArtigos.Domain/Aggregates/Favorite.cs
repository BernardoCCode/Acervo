using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class Favorite : AggregateRoot<FavoriteId>
{
    public UserId UserId { get; private set; }
    public ArticleId ArticleId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Favorite()
    {
    }

    public static Favorite Create(UserId userId, ArticleId articleId, DateTime? createdAtUtc = null)
    {
        var now = createdAtUtc ?? DateTime.UtcNow;

        var favorite = new Favorite
        {
            Id = FavoriteId.New(),
            UserId = userId,
            ArticleId = articleId,
            CreatedAtUtc = now
        };

        favorite.Raise(new ArticleFavorited(favorite.Id, userId, articleId, now));
        return favorite;
    }
}