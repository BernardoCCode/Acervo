using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Articles;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Services;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class ArticleService
{
    private readonly IArticleRepository _articles;
    private readonly IArticleContentRepository _contents;
    private readonly ICitationFormatter _citationFormatter;
    private readonly IPdfFetchPort _pdfFetch;
    private readonly IReadableContentExtractor _extractor;
    private readonly IAcademicSearchPort _academicSearch;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleService(
        IArticleRepository articles,
        IArticleContentRepository contents,
        ICitationFormatter citationFormatter,
        IPdfFetchPort pdfFetch,
        IReadableContentExtractor extractor,
        IAcademicSearchPort academicSearch,
        IUnitOfWork unitOfWork)
    {
        _articles = articles;
        _contents = contents;
        _citationFormatter = citationFormatter;
        _pdfFetch = pdfFetch;
        _extractor = extractor;
        _academicSearch = academicSearch;
        _unitOfWork = unitOfWork;
    }

    public async Task<ArticleDto> GetByIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetByIdAsync(
            ArticleId.From(articleId),
            cancellationToken);

        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        return article.ToDto();
    }

    public async Task<string> ExportCitationAsync(
        Guid articleId,
        CitationStyle style,
        CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetByIdAsync(
            ArticleId.From(articleId),
            cancellationToken);

        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        return _citationFormatter.Format(article, style);
    }

    public async Task<PdfDocumentContent> GetPdfAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var article = await _articles.GetByIdAsync(
            ArticleId.From(articleId),
            cancellationToken);

        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        var pdfUrl = article.PdfUrl?.ToString();
        if (string.IsNullOrWhiteSpace(pdfUrl))
            throw new NotFoundException("Este artigo não tem PDF disponível para leitura embutida.");

        var content = await _pdfFetch.FetchAsync(pdfUrl, cancellationToken);
        if (content is null)
            throw new NotFoundException("Não foi possível carregar o PDF deste artigo agora.");

        return content;
    }

    public async Task<ReadableContentDto> GetReadableContentAsync(
        Guid articleId,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var typedId = ArticleId.From(articleId);
        var article = await _articles.GetByIdAsync(typedId, cancellationToken);
        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        var cached = await _contents.GetByArticleIdAsync(typedId, cancellationToken);
        if (cached is not null
            && !forceRefresh
            && cached.Source is ReadableContentSource.PdfText or ReadableContentSource.HtmlPage
            && cached.Body.Trim().Length >= 400)
        {
            return ToReadableDto(article, cached, isFallback: false);
        }

        ExtractedReadableContent? extracted = null;

        var pdfUrl = article.PdfUrl?.ToString();
        if (!string.IsNullOrWhiteSpace(pdfUrl))
        {
            try
            {
                var pdf = await _pdfFetch.FetchAsync(pdfUrl, cancellationToken);
                if (pdf is not null)
                    extracted = await _extractor.FromPdfAsync(pdf.Bytes, cancellationToken);
            }
            catch
            {
                // Continue with the publisher page and alternate open-access copies.
            }
        }

        if (extracted is null
            || extracted.Source == ReadableContentSource.Abstract
            || extracted.Body.Trim().Length < 400)
        {
            var pageUrl = article.Publication.Url?.ToString();
            if (!string.IsNullOrWhiteSpace(pageUrl))
            {
                try
                {
                    extracted = await _extractor.FromHtmlUrlAsync(pageUrl, cancellationToken);
                }
                catch
                {
                    // Continue with alternate sources below.
                }
            }
        }

        // A provider may advertise a PDF that has moved or blocks automated access.
        // Search the same paper across the other academic sources and transparently
        // repair the cached PDF URL when an extractable copy is found.
        if (!IsReadable(extracted))
        {
            var alternatives = await _academicSearch.SearchAsync(
                article.Title,
                filters: null,
                cancellationToken);

            foreach (var candidate in alternatives.Take(8))
            {
                if (!IsSameWork(article, candidate))
                    continue;

                if (string.Equals(
                    candidate.PdfUrl,
                    article.PdfUrl?.ToString(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                extracted = await TryExtractAlternativeAsync(candidate, cancellationToken);
                if (!IsReadable(extracted))
                    continue;

                if (!string.IsNullOrWhiteSpace(candidate.PdfUrl))
                    article.UpdateMetadata(pdfUrl: candidate.PdfUrl);
                break;
            }
        }

        // Product rule: only full extractable text (PDF/HTML). Never surface abstract-only.
        if (!IsReadable(extracted))
        {
            return new ReadableContentDto(
                article.Id.Value,
                article.Title,
                string.Empty,
                [],
                ReadableContentSource.Abstract,
                null,
                true,
                null);
        }

        ArticleContent content;
        if (cached is null)
        {
            content = ArticleContent.Create(
                typedId,
                extracted.Body,
                extracted.Source,
                extracted.PageCount);
            await _contents.AddAsync(content, cancellationToken);
        }
        else
        {
            cached.Replace(extracted.Body, extracted.Source, extracted.PageCount);
            content = cached;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToReadableDto(
            article,
            content,
            isFallback: content.Source == ReadableContentSource.Abstract);
    }

    private async Task<ExtractedReadableContent?> TryExtractAlternativeAsync(
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
            return null;
        }
    }

    private static bool IsReadable(ExtractedReadableContent? content)
        => content is not null
            && content.Source is ReadableContentSource.PdfText or ReadableContentSource.HtmlPage
            && content.Body.Trim().Length >= 400;

    private static bool IsSameWork(Article article, AcademicArticleCandidate candidate)
    {
        var articleDoi = article.Publication.Doi?.Value;
        if (!string.IsNullOrWhiteSpace(articleDoi)
            && !string.IsNullOrWhiteSpace(candidate.Doi))
        {
            return articleDoi.Equals(candidate.Doi, StringComparison.OrdinalIgnoreCase);
        }

        var expected = TitleTerms(article.Title);
        var actual = TitleTerms(candidate.Title);
        if (expected.Count == 0 || actual.Count == 0)
            return false;

        var common = expected.Intersect(actual, StringComparer.OrdinalIgnoreCase).Count();
        return common / (double)Math.Max(expected.Count, actual.Count) >= 0.8;
    }

    private static HashSet<string> TitleTerms(string title)
        => title
            .ToLowerInvariant()
            .Split(
                [' ', '\t', '\r', '\n', ':', ';', ',', '.', '-', '–', '—', '(', ')'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 2)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static ReadableContentDto ToReadableDto(
        Article article,
        ArticleContent content,
        bool isFallback)
    {
        var paragraphs = SplitParagraphs(content.Body);
        var message = content.Source == ReadableContentSource.HtmlPage
            ? "Texto extraído da página da fonte."
            : null;

        return new ReadableContentDto(
            article.Id.Value,
            article.Title,
            content.Body,
            paragraphs,
            content.Source,
            content.PageCount,
            isFallback,
            message);
    }

    private static IReadOnlyList<string> SplitParagraphs(string body)
    {
        return body
            .Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => p.Length > 0)
            .ToList();
    }
}
