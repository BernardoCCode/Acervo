using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class SearchQuery : AggregateRoot<SearchQueryId>
{
    private readonly List<ArticleId> _resultArticleIds = [];

    public UserId? UserId { get; private set; }
    public string RawText { get; private set; } = null!;
    public SearchFilters Filters { get; private set; } = null!;
    public int ResultCount { get; private set; }
    public DateTime ExecutedAtUtc { get; private set; }
    public DateTime? LastAccessedAtUtc { get; private set; }

    public IReadOnlyCollection<ArticleId> ResultArticleIds => _resultArticleIds.AsReadOnly();

    private SearchQuery()
    {
    }

    public static SearchQuery Create(
        string rawText,
        SearchFilters? filters = null,
        UserId? userId = null,
        DateTime? executedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            throw new DomainException("Search query text is required.");

        var now = executedAtUtc ?? DateTime.UtcNow;

        return new SearchQuery
        {
            Id = SearchQueryId.New(),
            UserId = userId,
            RawText = rawText.Trim(),
            Filters = filters ?? SearchFilters.Empty(),
            ResultCount = 0,
            ExecutedAtUtc = now,
            LastAccessedAtUtc = now
        };
    }

    public void RecordResults(IEnumerable<ArticleId> articleIds, DateTime? accessedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(articleIds);

        _resultArticleIds.Clear();
        _resultArticleIds.AddRange(articleIds.Distinct());
        ResultCount = _resultArticleIds.Count;

        var now = accessedAtUtc ?? DateTime.UtcNow;
        LastAccessedAtUtc = now;
        Raise(new SearchPerformed(Id, UserId, RawText, ResultCount, now));
    }

    public void MarkAccessed(DateTime? accessedAtUtc = null)
    {
        LastAccessedAtUtc = accessedAtUtc ?? DateTime.UtcNow;
    }
}
