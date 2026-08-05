using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

/// <summary>
/// Cached readable body extracted for in-app reading (not a PDF viewer).
/// </summary>
public sealed class ArticleContent : AggregateRoot<ArticleId>
{
    public string Body { get; private set; } = null!;
    public ReadableContentSource Source { get; private set; }
    public int? PageCount { get; private set; }
    public DateTime ExtractedAtUtc { get; private set; }

    private ArticleContent()
    {
    }

    public static ArticleContent Create(
        ArticleId articleId,
        string body,
        ReadableContentSource source,
        int? pageCount = null,
        DateTime? extractedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Readable content body is required.");

        if (!Enum.IsDefined(source))
            throw new DomainException("Invalid readable content source.");

        return new ArticleContent
        {
            Id = articleId,
            Body = NormalizeBody(body),
            Source = source,
            PageCount = pageCount is > 0 ? pageCount : null,
            ExtractedAtUtc = extractedAtUtc ?? DateTime.UtcNow
        };
    }

    public void Replace(string body, ReadableContentSource source, int? pageCount = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Readable content body is required.");

        Body = NormalizeBody(body);
        Source = source;
        PageCount = pageCount is > 0 ? pageCount : null;
        ExtractedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeBody(string body)
    {
        var trimmed = body
            .Replace("\0", string.Empty, StringComparison.Ordinal)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Trim();
        while (trimmed.Contains("\n\n\n", StringComparison.Ordinal))
            trimmed = trimmed.Replace("\n\n\n", "\n\n", StringComparison.Ordinal);
        return trimmed;
    }
}
