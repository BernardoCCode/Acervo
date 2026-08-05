using PsiArtigos.Application.DTOs.Search;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class SearchService
{
    private const int MaxResultsToReturn = 30;

    private readonly IAcademicSearchPort _academicSearch;
    private readonly IArticleRepository _articles;
    private readonly ISearchQueryRepository _searchQueries;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public SearchService(
        IAcademicSearchPort academicSearch,
        IArticleRepository articles,
        ISearchQueryRepository searchQueries,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _academicSearch = academicSearch;
        _articles = articles;
        _searchQueries = searchQueries;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<SearchArticlesResult> SearchAsync(
        SearchArticlesRequest request,
        CancellationToken cancellationToken = default)
    {
        var filters = ToDomainFilters(request.Filters);

        var candidates = await _academicSearch.SearchAsync(
            request.Query,
            request.Filters,
            cancellationToken);

        // Sources already restrict to open PDFs (OpenAlex has_content.pdf, Europe PMC HAS_PDF,
        // Semantic Scholar openAccessPdf, arXiv). Extract full text when the reader opens.
        // The port returns candidates ranked by relevance to the query — keep that order.
        candidates = candidates
            .Where(c => !string.IsNullOrWhiteSpace(c.PdfUrl))
            .Take(MaxResultsToReturn)
            .ToList();

        var dois = candidates
            .Select(c => c.Doi)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Cast<string>()
            .ToList();

        var existingByDoi = (await _articles.GetByDoisAsync(dois, cancellationToken))
            .Where(a => a.Publication.Doi is not null)
            .ToDictionary(
                a => a.Publication.Doi!.Value,
                a => a,
                StringComparer.OrdinalIgnoreCase);

        var persisted = new List<Article>();

        foreach (var candidate in candidates)
        {
            Article? existing = null;
            if (!string.IsNullOrWhiteSpace(candidate.Doi)
                && existingByDoi.TryGetValue(candidate.Doi, out var match))
            {
                existing = match;
            }

            if (existing is not null)
            {
                existing.UpdateMetadata(
                    abstractText: candidate.Abstract,
                    citationCount: candidate.CitationCount,
                    studyType: candidate.StudyType,
                    language: candidate.Language,
                    pdfUrl: candidate.PdfUrl);

                persisted.Add(existing);
                continue;
            }

            var article = candidate.ToArticle();
            await _articles.AddAsync(article, cancellationToken);
            persisted.Add(article);

            if (!string.IsNullOrWhiteSpace(candidate.Doi) && article.Publication.Doi is not null)
                existingByDoi[article.Publication.Doi.Value] = article;
        }

        if (request.Filters?.MinCitations is int minCitations)
            persisted = persisted.Where(a => a.CitationCount >= minCitations).ToList();

        var searchQuery = SearchQuery.Create(
            request.Query,
            filters,
            _currentUser.UserId);

        searchQuery.RecordResults(persisted.Select(a => a.Id));
        await _searchQueries.AddAsync(searchQuery, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SearchArticlesResult(
            searchQuery.Id.Value,
            searchQuery.RawText,
            persisted.Count,
            persisted.Select(a => a.ToDto()).ToList());
    }

    public async Task<IReadOnlyList<SearchHistoryItemDto>> ListHistoryAsync(
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var history = await _searchQueries.ListRecentByUserAsync(userId, take, cancellationToken);

        return history
            .Select(q => new SearchHistoryItemDto(
                q.Id.Value,
                q.RawText,
                q.ResultCount,
                q.ExecutedAtUtc,
                q.LastAccessedAtUtc))
            .ToList();
    }

    private static SearchFilters ToDomainFilters(SearchFiltersRequest? filters)
    {
        if (filters is null)
            return SearchFilters.Empty();

        return SearchFilters.Create(
            filters.YearMin,
            filters.YearMax,
            filters.Language,
            filters.StudyType,
            filters.MinCitations,
            filters.Sources);
    }
}
