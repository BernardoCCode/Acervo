using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Events;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Aggregates;

public sealed class GeneratedInsight : AggregateRoot<InsightId>
{
    public UserId UserId { get; private set; }
    public ArticleId ArticleId { get; private set; }
    public InsightType Type { get; private set; }
    public string Content { get; private set; } = null!;
    public string? SourceLanguage { get; private set; }
    public string? TargetLanguage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private GeneratedInsight()
    {
    }

    public static GeneratedInsight CreateSummary(
        UserId userId,
        ArticleId articleId,
        string content,
        string? sourceLanguage = null,
        DateTime? createdAtUtc = null)
        => Create(userId, articleId, InsightType.Summary, content, sourceLanguage, null, createdAtUtc);

    public static GeneratedInsight CreateBeginnerExplanation(
        UserId userId,
        ArticleId articleId,
        string content,
        string? sourceLanguage = null,
        DateTime? createdAtUtc = null)
        => Create(userId, articleId, InsightType.BeginnerExplanation, content, sourceLanguage, null, createdAtUtc);

    public static GeneratedInsight CreateTranslation(
        UserId userId,
        ArticleId articleId,
        string content,
        string sourceLanguage,
        string targetLanguage,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguage))
            throw new DomainException("Source language is required for translation.");

        if (string.IsNullOrWhiteSpace(targetLanguage))
            throw new DomainException("Target language is required for translation.");

        return Create(
            userId,
            articleId,
            InsightType.Translation,
            content,
            sourceLanguage,
            targetLanguage,
            createdAtUtc);
    }

    private static GeneratedInsight Create(
        UserId userId,
        ArticleId articleId,
        InsightType type,
        string content,
        string? sourceLanguage,
        string? targetLanguage,
        DateTime? createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Generated insight content is required.");

        if (!Enum.IsDefined(type))
            throw new DomainException("Invalid insight type.");

        var now = createdAtUtc ?? DateTime.UtcNow;

        var insight = new GeneratedInsight
        {
            Id = InsightId.New(),
            UserId = userId,
            ArticleId = articleId,
            Type = type,
            Content = content.Trim(),
            SourceLanguage = NormalizeLanguage(sourceLanguage),
            TargetLanguage = NormalizeLanguage(targetLanguage),
            CreatedAtUtc = now
        };

        insight.Raise(new InsightGenerated(insight.Id, userId, articleId, type, now));
        return insight;
    }

    public void EnsureOwnedBy(UserId userId)
    {
        if (UserId != userId)
            throw new DomainException("Generated insight does not belong to this user.");
    }

    private static string? NormalizeLanguage(string? language)
        => string.IsNullOrWhiteSpace(language) ? null : language.Trim().ToLowerInvariant();
}
