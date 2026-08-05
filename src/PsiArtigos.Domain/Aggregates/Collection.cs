using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class Collection : AggregateRoot<CollectionId>
{
    private readonly List<CollectionItem> _items = [];

    public UserId UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CollectionItem> Items => _items.AsReadOnly();

    private Collection()
    {
    }

    public static Collection Create(
        UserId userId,
        string name,
        string? description = null,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Collection name is required.");

        var now = createdAtUtc ?? DateTime.UtcNow;

        return new Collection
        {
            Id = CollectionId.New(),
            UserId = userId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Collection name is required.");

        Name = name.Trim();
        Touch();
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Touch();
    }

    public bool AddArticle(ArticleId articleId)
    {
        if (_items.Any(i => i.ArticleId == articleId))
            return false;

        var now = DateTime.UtcNow;
        _items.Add(CollectionItem.Create(articleId, now));
        Raise(new ArticleAddedToCollection(Id, UserId, articleId, now));
        Touch();
        return true;
    }

    public bool RemoveArticle(ArticleId articleId)
    {
        var removed = _items.RemoveAll(i => i.ArticleId == articleId) > 0;

        if (removed)
            Touch();

        return removed;
    }

    public bool Contains(ArticleId articleId) => _items.Any(i => i.ArticleId == articleId);

    public void EnsureOwnedBy(UserId userId)
    {
        if (UserId != userId)
            throw new DomainException("Collection does not belong to this user.");
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}