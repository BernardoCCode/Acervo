using System.Text.RegularExpressions;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class RecommendationEngineService
{
    private readonly IRecommendationRepository _recommendations;
    private readonly IReadingSessionRepository _sessions;
    private readonly IFavoriteRepository _favorites;
    private readonly ISearchQueryRepository _searches;
    private readonly IUserRepository _users;
    private readonly IArticleRepository _articles;
    private readonly IAcademicSearchPort _academicSearch;
    private readonly IAiRecommendationPort _ai;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RecommendationEngineService(
        IRecommendationRepository recommendations,
        IReadingSessionRepository sessions,
        IFavoriteRepository favorites,
        ISearchQueryRepository searches,
        IUserRepository users,
        IArticleRepository articles,
        IAcademicSearchPort academicSearch,
        IAiRecommendationPort ai,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _recommendations = recommendations;
        _sessions = sessions;
        _favorites = favorites;
        _searches = searches;
        _users = users;
        _articles = articles;
        _academicSearch = academicSearch;
        _ai = ai;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public Task<int> RefreshAsync(CancellationToken cancellationToken = default)
        => RefreshForUserAsync(_currentUser.GetRequiredUserId(), cancellationToken);

    public async Task<int> RefreshForUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        var sessions = await _sessions.ListRecentByUserAsync(userId, 50, cancellationToken);
        var favorites = await _favorites.ListByUserAsync(userId, cancellationToken);
        var searches = await _searches.ListRecentByUserAsync(userId, 20, cancellationToken);
        var previous = await _recommendations.ListByUserAsync(userId, cancellationToken);

        var readIds = sessions.Select(x => x.ArticleId).ToHashSet();
        var favoriteIds = favorites.Select(x => x.ArticleId).ToHashSet();
        var dismissedIds = previous.Where(x => x.IsDismissed).Select(x => x.ArticleId).ToHashSet();
        var sourceIds = readIds.Concat(favoriteIds).Distinct().ToList();
        var sourceArticles = await _articles.GetByIdsAsync(sourceIds, cancellationToken);
        var sourceById = sourceArticles.ToDictionary(x => x.Id);
        var weightedSources = sessions
            .Where(x => sourceById.ContainsKey(x.ArticleId))
            .Select(x => new WeightedSource(
                sourceById[x.ArticleId],
                EngagementWeight(x, favoriteIds.Contains(x.ArticleId))))
            .Concat(sourceArticles
                .Where(x => favoriteIds.Contains(x.Id) && sessions.All(s => s.ArticleId != x.Id))
                .Select(x => new WeightedSource(x, 0.9)))
            .OrderByDescending(x => x.Weight)
            .Take(15)
            .ToList();

        var input = new RecommendationProfileInput(
            weightedSources.Select(x => x.Article.Title).Take(10).ToList(),
            searches.Select(x => x.RawText).Take(10).ToList(),
            user?.Profile.Interests.Select(x => x.Value).ToList() ?? []);
        var plan = await AnalyzeWithFallbackAsync(input, cancellationToken);
        var profileTerms = BuildProfileTerms(plan, weightedSources, searches.Select(x => x.RawText));
        var queries = plan.SearchQueries
            .Concat(searches.Select(x => x.RawText))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();
        if (queries.Count == 0)
        {
            queries =
            [
                "evidence based psychology systematic review",
                "cognitive neuroscience recent advances",
                "mental health interventions meta analysis"
            ];
        }

        var rawCandidates = new List<(AcademicArticleCandidate Candidate, string Query)>();
        foreach (var query in queries)
        {
            try
            {
                var found = await _academicSearch.SearchAsync(query, null, cancellationToken);
                rawCandidates.AddRange(found.Take(10).Select(x => (x, query)));
            }
            catch when (!cancellationToken.IsCancellationRequested)
            {
                // One provider/query should not prevent the rest of the feed.
            }
        }

        var candidateScores = rawCandidates
            .GroupBy(x => CandidateKey(x.Candidate), StringComparer.OrdinalIgnoreCase)
            .Select(group => ScoreCandidate(
                group.First().Candidate,
                group.Select(x => x.Query).ToList(),
                profileTerms,
                weightedSources))
            .Where(x => x.TopicScore >= 0.12)
            .OrderByDescending(x => x.Score)
            .Take(35)
            .ToList();

        var selected = Diversify(candidateScores, 12);
        await _recommendations.RemoveActiveByUserAsync(userId, cancellationToken);
        var created = 0;
        foreach (var scored in selected)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var article = await ImportReadableAsync(scored.Candidate, cancellationToken);
            if (article is null || readIds.Contains(article.Id) || dismissedIds.Contains(article.Id))
                continue;

            var source = BestSource(scored.Candidate, weightedSources);
            var topic = plan.Topics.FirstOrDefault(t =>
                Tokenize(scored.Candidate.Title + " " + scored.Candidate.Abstract)
                    .Overlaps(Tokenize(t)));
            var explanation = source is not null
                ? $"Porque você se envolveu com “{Trim(source.Article.Title, 85)}”; este artigo aprofunda {topic ?? "conceitos relacionados"}."
                : $"Relacionado ao seu interesse em {topic ?? plan.Topics.FirstOrDefault() ?? "pesquisa acadêmica recente"}, com texto completo disponível.";
            var reason = source is not null
                ? RecommendationReason.SemanticSimilarity
                : RecommendationReason.TopicMatch;
            await _recommendations.AddAsync(
                Recommendation.Create(
                    userId,
                    article.Id,
                    reason,
                    scored.Score,
                    explanation,
                    source?.Article.Id,
                    scored.TopicScore,
                    scored.EngagementScore,
                    scored.QualityScore,
                    scored.FreshnessScore),
                cancellationToken);
            created++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return created;
    }

    private async Task<RecommendationProfilePlan> AnalyzeWithFallbackAsync(
        RecommendationProfileInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _ai.AnalyzeAsync(input, cancellationToken);
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
            var seeds = input.DeclaredInterests
                .Concat(input.RecentSearches)
                .Concat(input.StrongArticleTitles)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();
            return new RecommendationProfilePlan(
                seeds.Take(5).ToList(),
                seeds.Select(x => $"{x} systematic review recent advances").Take(5).ToList(),
                "Perfil calculado localmente a partir do histórico.");
        }
    }

    private async Task<Article?> ImportReadableAsync(
        AcademicArticleCandidate candidate,
        CancellationToken cancellationToken)
    {
        // Metadata-first: recommendations only need open-access PDF URLs.
        // Full-text extraction happens lazily when the reader opens the article.
        if (string.IsNullOrWhiteSpace(candidate.PdfUrl))
            return null;

        Article? article = null;
        if (!string.IsNullOrWhiteSpace(candidate.Doi))
            article = await _articles.GetByDoiAsync(candidate.Doi, cancellationToken);

        if (article is null)
        {
            article = candidate.ToArticle();
            await _articles.AddAsync(article, cancellationToken);
            return article;
        }

        article.UpdateMetadata(
            candidate.Abstract,
            candidate.CitationCount,
            candidate.StudyType,
            candidate.Language,
            candidate.PdfUrl);
        return article;
    }

    private static CandidateScore ScoreCandidate(
        AcademicArticleCandidate candidate,
        IReadOnlyList<string> matchingQueries,
        HashSet<string> profileTerms,
        IReadOnlyList<WeightedSource> sources)
    {
        var terms = Tokenize(candidate.Title + " " + candidate.Abstract);
        var titleTerms = Tokenize(candidate.Title);
        var common = terms.Count == 0 ? 0 : terms.Count(profileTerms.Contains);
        var titleCommon = titleTerms.Count == 0 ? 0 : titleTerms.Count(profileTerms.Contains);
        var queryBoost = matchingQueries
            .Select(q => Similarity(terms, Tokenize(q)))
            .DefaultIfEmpty(0)
            .Max();
        var topic = Math.Clamp(
            common / (double)Math.Max(5, Math.Min(profileTerms.Count, 20)) * 0.55
            + titleCommon / (double)Math.Max(2, titleTerms.Count) * 0.25
            + queryBoost * 0.2,
            0,
            1);
        var sourceFit = sources
            .Select(x => Similarity(terms, Tokenize(x.Article.Title + " " + x.Article.Abstract)) * x.Weight)
            .DefaultIfEmpty(0)
            .Max();
        var quality = Math.Clamp(Math.Log10(candidate.CitationCount + 1) / 3.0, 0, 1);
        var age = candidate.Year is null ? 8 : Math.Max(0, DateTime.UtcNow.Year - candidate.Year.Value);
        var freshness = Math.Exp(-age / 8.0);
        var score = Math.Clamp(
            topic * 0.5 + sourceFit * 0.25 + quality * 0.15 + freshness * 0.1,
            0,
            1);
        return new CandidateScore(candidate, score, topic, sourceFit, quality, freshness, terms);
    }

    private static IReadOnlyList<CandidateScore> Diversify(
        IReadOnlyList<CandidateScore> candidates,
        int take)
    {
        var remaining = candidates.ToList();
        var selected = new List<CandidateScore>();
        while (remaining.Count > 0 && selected.Count < take)
        {
            var best = remaining
                .OrderByDescending(candidate =>
                    candidate.Score
                    - 0.22 * selected
                        .Select(chosen => Similarity(candidate.Terms, chosen.Terms))
                        .DefaultIfEmpty(0)
                        .Max())
                .First();
            selected.Add(best);
            remaining.Remove(best);
        }
        return selected;
    }

    private static HashSet<string> BuildProfileTerms(
        RecommendationProfilePlan plan,
        IReadOnlyList<WeightedSource> sources,
        IEnumerable<string> searches)
        => Tokenize(string.Join(
            " ",
            plan.Topics
                .Concat(plan.SearchQueries)
                .Concat(searches)
                .Concat(sources.SelectMany(x =>
                    Enumerable.Repeat(x.Article.Title + " " + x.Article.Abstract, Math.Max(1, (int)Math.Round(x.Weight * 3)))))));

    private static WeightedSource? BestSource(
        AcademicArticleCandidate candidate,
        IReadOnlyList<WeightedSource> sources)
    {
        var candidateTerms = Tokenize(candidate.Title + " " + candidate.Abstract);
        return sources
            .Select(x => (Source: x, Fit: Similarity(
                candidateTerms,
                Tokenize(x.Article.Title + " " + x.Article.Abstract)) * x.Weight))
            .Where(x => x.Fit >= 0.08)
            .OrderByDescending(x => x.Fit)
            .Select(x => x.Source)
            .FirstOrDefault();
    }

    private static double EngagementWeight(ReadingSession session, bool favorite)
    {
        var recencyDays = Math.Max(0, (DateTime.UtcNow - session.LastOpenedAtUtc).TotalDays);
        var recency = Math.Exp(-recencyDays / 60);
        var progress = session.Progress.Percent / 100;
        var active = Math.Clamp(session.ActiveReadingSeconds / 900.0, 0, 1);
        var highlights = Math.Clamp(session.Highlights.Count / 3.0, 0, 1);
        return Math.Clamp(
            progress * 0.3
            + active * 0.2
            + highlights * 0.15
            + (session.IsCompleted ? 0.15 : 0)
            + (favorite ? 0.2 : 0),
            0.05,
            1) * recency;
    }

    private static HashSet<string> Tokenize(string? value)
    {
        var stop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "with", "from", "that", "this", "uma", "para",
            "com", "dos", "das", "sobre", "study", "analysis", "article"
        };
        return Regex.Matches((value ?? string.Empty).ToLowerInvariant(), @"[\p{L}\p{N}]{3,}")
            .Select(x => x.Value)
            .Where(x => !stop.Contains(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static double Similarity(HashSet<string> left, HashSet<string> right)
    {
        if (left.Count == 0 || right.Count == 0)
            return 0;
        var intersection = left.Count(right.Contains);
        return intersection / (double)(left.Count + right.Count - intersection);
    }

    private static string CandidateKey(AcademicArticleCandidate candidate)
        => !string.IsNullOrWhiteSpace(candidate.Doi)
            ? $"doi:{candidate.Doi.Trim().ToLowerInvariant()}"
            : $"title:{string.Join(' ', Tokenize(candidate.Title).Order())}";

    private static string Trim(string value, int max)
        => value.Length <= max ? value : value[..(max - 1)] + "…";

    private sealed record WeightedSource(Article Article, double Weight);

    private sealed record CandidateScore(
        AcademicArticleCandidate Candidate,
        double Score,
        double TopicScore,
        double EngagementScore,
        double QualityScore,
        double FreshnessScore,
        HashSet<string> Terms);
}
