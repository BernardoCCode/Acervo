using PsiArtigos.Domain.Abstractions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class CollectionItem : ValueObject
{
    public ArticleId ArticleId { get; private set; }
    public DateTime AddedAtUtc { get; private set; }

    private CollectionItem()
    {
    }

    private CollectionItem(ArticleId articleId, DateTime addedAtUtc)
    {
        ArticleId = articleId;
        AddedAtUtc = addedAtUtc;
    }

    public static CollectionItem Create(ArticleId articleId, DateTime? addedAtUtc = null)
        => new(articleId, addedAtUtc ?? DateTime.UtcNow);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ArticleId;
    }
}
