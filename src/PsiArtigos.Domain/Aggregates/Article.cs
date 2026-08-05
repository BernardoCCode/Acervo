using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class Article : AggregateRoot<ArticleId>
{
    private readonly List<Author> _authors = [];
    private readonly List<ExternalReference> _externalReferences = [];
    private readonly List<TopicTag> _topics = [];

    public string Title { get; private set; } = null!;
    public string? Abstract { get; private set; }
    public PublicationInfo Publication { get; private set; } = null!;
    public SourceSystem PrimarySource { get; private set; }
    public int CitationCount { get; private set; }
    public string? Language { get; private set; }
    public StudyType StudyType { get; private set; }
    public Uri? PdfUrl { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<Author> Authors => _authors.AsReadOnly();
    public IReadOnlyCollection<ExternalReference> ExternalReferences => _externalReferences.AsReadOnly();
    public IReadOnlyCollection<TopicTag> Topics => _topics.AsReadOnly();

    private Article()
    {
    }

    public static Article Create(
        string title,
        SourceSystem primarySource,
        PublicationInfo publication,
        IEnumerable<ExternalReference>? externalReferences = null,
        IEnumerable<Author>? authors = null,
        string? abstractText = null,
        int citationCount = 0,
        string? language = null,
        StudyType studyType = StudyType.Unknown,
        string? pdfUrl = null,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Article title is required.");

        if (!Enum.IsDefined(primarySource))
            throw new DomainException("Invalid primary source.");

        if (citationCount < 0)
            throw new DomainException("Citation count cannot be negative.");

        var references = (externalReferences ?? []).Distinct().ToList();
        var hasStableIdentity = references.Count > 0 || publication.Doi is not null || publication.Url is not null;

        if (!hasStableIdentity)
            throw new DomainException("Article requires at least one external reference, DOI, or URL.");

        Uri? parsedPdfUrl = null;
        if (!string.IsNullOrWhiteSpace(pdfUrl))
        {
            if (!Uri.TryCreate(pdfUrl.Trim(), UriKind.Absolute, out parsedPdfUrl)
                || (parsedPdfUrl.Scheme != Uri.UriSchemeHttp && parsedPdfUrl.Scheme != Uri.UriSchemeHttps))
            {
                throw new DomainException("PDF url must be an absolute http/https url.");
            }
        }

        var now = createdAtUtc ?? DateTime.UtcNow;

        var article = new Article
        {
            Id = ArticleId.New(),
            Title = title.Trim(),
            Abstract = string.IsNullOrWhiteSpace(abstractText) ? null : abstractText.Trim(),
            Publication = publication,
            PrimarySource = primarySource,
            CitationCount = citationCount,
            Language = string.IsNullOrWhiteSpace(language) ? null : language.Trim().ToLowerInvariant(),
            StudyType = studyType,
            PdfUrl = parsedPdfUrl,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        article._authors.AddRange((authors ?? []).Distinct());
        article._externalReferences.AddRange(references);
        article.Raise(new ArticleIndexed(article.Id, now));

        return article;
    }

    public void UpdateMetadata(
        string? abstractText = null,
        int? citationCount = null,
        StudyType? studyType = null,
        string? language = null,
        string? pdfUrl = null)
    {
        if (citationCount is < 0)
            throw new DomainException("Citation count cannot be negative.");

        if (abstractText is not null)
            Abstract = string.IsNullOrWhiteSpace(abstractText) ? null : abstractText.Trim();

        if (citationCount is not null)
            CitationCount = citationCount.Value;

        if (studyType is not null)
            StudyType = studyType.Value;

        if (language is not null)
            Language = string.IsNullOrWhiteSpace(language) ? null : language.Trim().ToLowerInvariant();

        if (pdfUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(pdfUrl))
            {
                PdfUrl = null;
            }
            else if (!Uri.TryCreate(pdfUrl.Trim(), UriKind.Absolute, out var parsedPdfUrl)
                     || (parsedPdfUrl.Scheme != Uri.UriSchemeHttp && parsedPdfUrl.Scheme != Uri.UriSchemeHttps))
            {
                throw new DomainException("PDF url must be an absolute http/https url.");
            }
            else
            {
                PdfUrl = parsedPdfUrl;
            }
        }

        Touch();
    }

    public void AddAuthor(Author author)
    {
        ArgumentNullException.ThrowIfNull(author);

        if (_authors.Contains(author))
            return;

        _authors.Add(author);
        Touch();
    }

    public void AddExternalReference(ExternalReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        if (_externalReferences.Contains(reference))
            return;

        _externalReferences.Add(reference);
        Touch();
    }

    public void AddTopic(TopicTag topic)
    {
        ArgumentNullException.ThrowIfNull(topic);

        if (_topics.Contains(topic))
            return;

        _topics.Add(topic);
        Touch();
    }

    public void ReplaceTopics(IEnumerable<TopicTag> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);

        _topics.Clear();
        _topics.AddRange(topics.Distinct());
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}