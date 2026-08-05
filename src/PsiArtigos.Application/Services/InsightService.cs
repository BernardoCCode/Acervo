using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Insights;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class InsightService
{
    private readonly IAiInsightPort _aiInsights;
    private readonly IArticleRepository _articles;
    private readonly IArticleContentRepository _contents;
    private readonly IGeneratedInsightRepository _insights;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public InsightService(
        IAiInsightPort aiInsights,
        IArticleRepository articles,
        IArticleContentRepository contents,
        IGeneratedInsightRepository insights,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _aiInsights = aiInsights;
        _articles = articles;
        _contents = contents;
        _insights = insights;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<InsightDto> GenerateAsync(
        GenerateInsightRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var articleId = ArticleId.From(request.ArticleId);

        var article = await _articles.GetByIdAsync(articleId, cancellationToken);
        if (article is null)
            throw NotFoundException.For<Article>(request.ArticleId);

        var hasFocus = !string.IsNullOrWhiteSpace(request.FocusText);

        // Selection-focused insights should not reuse the whole-article cache.
        if (!hasFocus)
        {
            var existing = await _insights.GetLatestAsync(
                userId,
                articleId,
                request.Type,
                cancellationToken);

            if (existing is not null)
            {
                var reusable =
                    request.Type != InsightType.Translation
                    || string.Equals(
                        existing.TargetLanguage,
                        request.TargetLanguage,
                        StringComparison.OrdinalIgnoreCase);
                if (reusable)
                    return ToDto(existing);
            }
        }

        string? sourceText;
        if (hasFocus)
        {
            sourceText = request.FocusText!.Trim();
            if (sourceText.Length > 4000)
                sourceText = sourceText[..4000];
        }
        else
        {
            var readable = await _contents.GetByArticleIdAsync(articleId, cancellationToken);
            sourceText = readable?.Body;
            if (string.IsNullOrWhiteSpace(sourceText))
                sourceText = article.Abstract;
            if (!string.IsNullOrWhiteSpace(sourceText))
            {
                var maxLength = request.Type == InsightType.Translation ? 80_000 : 6_000;
                if (sourceText.Length > maxLength)
                    sourceText = sourceText[..maxLength];
            }
        }

        var content = request.Type switch
        {
            InsightType.Summary => await _aiInsights.SummarizeAsync(
                article.Title,
                sourceText,
                cancellationToken),

            InsightType.BeginnerExplanation => await _aiInsights.ExplainForBeginnersAsync(
                article.Title,
                sourceText,
                cancellationToken),

            InsightType.Translation => await CreateTranslationAsync(
                article,
                sourceText,
                request,
                cancellationToken),

            _ => throw new DomainException("Unsupported insight type.")
        };

        // Ephemeral response for selection-focused AI — do not pollute article cache.
        if (hasFocus)
        {
            return new InsightDto(
                Guid.NewGuid(),
                article.Id.Value,
                request.Type,
                content,
                request.SourceLanguage ?? article.Language,
                request.Type == InsightType.Translation ? request.TargetLanguage : null,
                DateTime.UtcNow);
        }

        var insight = request.Type switch
        {
            InsightType.Summary => GeneratedInsight.CreateSummary(
                userId,
                articleId,
                content,
                article.Language),

            InsightType.BeginnerExplanation => GeneratedInsight.CreateBeginnerExplanation(
                userId,
                articleId,
                content,
                article.Language),

            InsightType.Translation => GeneratedInsight.CreateTranslation(
                userId,
                articleId,
                content,
                request.SourceLanguage ?? article.Language ?? "en",
                request.TargetLanguage!),

            _ => throw new DomainException("Unsupported insight type.")
        };

        await _insights.AddAsync(insight, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(insight);
    }

    private async Task<string> CreateTranslationAsync(
        Article article,
        string? sourceText,
        GenerateInsightRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
            throw new DomainException("Target language is required for translation.");

        var sourceLanguage = request.SourceLanguage ?? article.Language ?? "en";

        return await _aiInsights.TranslateAsync(
            article.Title,
            sourceText ?? article.Abstract,
            sourceLanguage,
            request.TargetLanguage,
            cancellationToken);
    }

    private static InsightDto ToDto(GeneratedInsight insight)
    {
        return new InsightDto(
            insight.Id.Value,
            insight.ArticleId.Value,
            insight.Type,
            insight.Content,
            insight.SourceLanguage,
            insight.TargetLanguage,
            insight.CreatedAtUtc);
    }
}
