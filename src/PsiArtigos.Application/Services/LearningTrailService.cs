using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Learning;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class LearningTrailService
{
    private readonly IAiLearningPort _aiLearning;
    private readonly IAcademicSearchPort _academicSearch;
    private readonly IArticleRepository _articles;
    private readonly IArticleContentRepository _contents;
    private readonly IPdfFetchPort _pdfFetch;
    private readonly IReadableContentExtractor _extractor;
    private readonly ILearningTrailRepository _trails;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public LearningTrailService(
        IAiLearningPort aiLearning,
        IAcademicSearchPort academicSearch,
        IArticleRepository articles,
        IArticleContentRepository contents,
        IPdfFetchPort pdfFetch,
        IReadableContentExtractor extractor,
        ILearningTrailRepository trails,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _aiLearning = aiLearning;
        _academicSearch = academicSearch;
        _articles = articles;
        _contents = contents;
        _pdfFetch = pdfFetch;
        _extractor = extractor;
        _trails = trails;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningTrailDto> CreateAsync(
        CreateLearningTrailRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        LearningTrailPlan plan;
        try
        {
            plan = await _aiLearning.PlanTrailAsync(request.Prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            var failedTrail = LearningTrail.Create(userId, request.Prompt, "Unknown");
            failedTrail.MarkFailed($"Failed to plan learning trail: {ex.Message}");
            await _trails.AddAsync(failedTrail, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return failedTrail.ToDto();
        }

        var trail = LearningTrail.Create(userId, request.Prompt, plan.Topic);

        try
        {
            var usedCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var stepPlan in plan.Steps)
            {
                var article = await ResolveArticleForStepAsync(
                    [
                        stepPlan.SearchQuery,
                        $"{plan.Topic} {stepPlan.Title}"
                    ],
                    usedCandidates,
                    cancellationToken);
                if (article is null)
                    continue;

                trail.AddStep(
                    stepPlan.Title,
                    stepPlan.Difficulty,
                    article.Id,
                    stepPlan.Rationale);
            }

            if (trail.Steps.Count >= 3 && trail.Steps.All(s => s.HasArticle))
                trail.MarkReady();
            else
                trail.MarkFailed(
                    "Não encontramos artigos abertos e legíveis suficientes para esta trilha. "
                    + "Tente descrever o tema com outros termos.");
        }
        catch (Exception ex)
        {
            trail.MarkFailed($"Failed to build learning trail steps: {ex.Message}");
        }

        await _trails.AddAsync(trail, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoWithArticlesAsync(trail, cancellationToken);
    }

    public async Task<LearningTrailDto> GetByIdAsync(
        Guid trailId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var trail = await _trails.GetByIdAsync(
            LearningTrailId.From(trailId),
            cancellationToken);

        if (trail is null)
            throw NotFoundException.For<LearningTrail>(trailId);

        trail.EnsureOwnedBy(userId);
        return await ToDtoWithArticlesAsync(trail, cancellationToken);
    }

    public async Task<IReadOnlyList<LearningTrailDto>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var trails = await _trails.ListByUserAsync(userId, cancellationToken);

        var ordered = trails.OrderByDescending(t => t.CreatedAtUtc).ToList();
        var articleIds = ordered
            .SelectMany(t => t.Steps)
            .Where(s => s.ArticleId is not null)
            .Select(s => s.ArticleId!.Value)
            .Distinct()
            .ToList();
        var articles = await _articles.GetByIdsAsync(articleIds, cancellationToken);
        var titles = articles.ToDictionary(a => a.Id.Value, a => a.Title);

        return ordered.Select(t => t.ToDto(titles)).ToList();
    }

    private async Task<Article?> ResolveArticleForStepAsync(
        IReadOnlyList<string> searchQueries,
        HashSet<string> usedCandidates,
        CancellationToken cancellationToken)
    {
        foreach (var searchQuery in searchQueries.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var candidates = await _academicSearch.SearchAsync(
                searchQuery,
                filters: null,
                cancellationToken);

            foreach (var candidate in candidates.Take(8))
            {
                var key = CandidateKey(candidate);
                if (!usedCandidates.Add(key))
                    continue;

                Article? article = null;
                if (!string.IsNullOrWhiteSpace(candidate.Doi))
                    article = await _articles.GetByDoiAsync(candidate.Doi, cancellationToken);

                var cached = article is null
                    ? null
                    : await _contents.GetByArticleIdAsync(article.Id, cancellationToken);
                if (cached is not null
                    && cached.Source is ReadableContentSource.PdfText or ReadableContentSource.HtmlPage
                    && cached.Body.Trim().Length >= 400)
                {
                    return article;
                }

                var extracted = await TryExtractCandidateAsync(candidate, cancellationToken);
                if (extracted is null)
                    continue;

                if (article is null)
                {
                    article = candidate.ToArticle();
                    await _articles.AddAsync(article, cancellationToken);
                }
                else
                {
                    article.UpdateMetadata(
                        abstractText: candidate.Abstract,
                        citationCount: candidate.CitationCount,
                        studyType: candidate.StudyType,
                        language: candidate.Language,
                        pdfUrl: candidate.PdfUrl);
                }

                if (cached is null)
                {
                    await _contents.AddAsync(
                        ArticleContent.Create(
                            article.Id,
                            extracted.Body,
                            extracted.Source,
                            extracted.PageCount),
                        cancellationToken);
                }
                else
                {
                    cached.Replace(extracted.Body, extracted.Source, extracted.PageCount);
                }

                return article;
            }
        }

        return null;
    }

    private async Task<ExtractedReadableContent?> TryExtractCandidateAsync(
        AcademicArticleCandidate candidate,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            ExtractedReadableContent? extracted = null;
            if (!string.IsNullOrWhiteSpace(candidate.PdfUrl))
            {
                var pdf = await _pdfFetch.FetchAsync(candidate.PdfUrl, timeout.Token);
                if (pdf is not null)
                    extracted = await _extractor.FromPdfAsync(pdf.Bytes, timeout.Token);
            }

            if (!IsReadable(extracted) && !string.IsNullOrWhiteSpace(candidate.Url))
                extracted = await _extractor.FromHtmlUrlAsync(candidate.Url, timeout.Token);

            return IsReadable(extracted) ? extracted : null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch
        {
            // A broken publisher URL must not fail the whole trail; try the next paper.
            return null;
        }
    }

    private static bool IsReadable(ExtractedReadableContent? content)
        => content is not null
            && content.Source is ReadableContentSource.PdfText or ReadableContentSource.HtmlPage
            && content.Body.Trim().Length >= 400;

    private async Task<LearningTrailDto> ToDtoWithArticlesAsync(
        LearningTrail trail,
        CancellationToken cancellationToken)
    {
        var articleIds = trail.Steps
            .Where(s => s.ArticleId is not null)
            .Select(s => s.ArticleId!.Value)
            .Distinct()
            .ToList();
        var articles = await _articles.GetByIdsAsync(articleIds, cancellationToken);
        var titles = articles.ToDictionary(a => a.Id.Value, a => a.Title);
        return trail.ToDto(titles);
    }

    private static string CandidateKey(AcademicArticleCandidate candidate)
        => !string.IsNullOrWhiteSpace(candidate.Doi)
            ? $"doi:{candidate.Doi.Trim()}"
            : $"title:{candidate.Title.Trim()}";
}
