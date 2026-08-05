using PsiArtigos.Application.DTOs.Articles;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Application.Interfaces;

namespace PsiArtigos.Application.Mapping;

public static class ArticleMapping
{
    public static ArticleDto ToDto(this Article article)
    {
        return new ArticleDto(
            article.Id.Value,
            article.Title,
            article.Abstract,
            article.Authors.Select(a => a.Name).ToList(),
            article.Publication.Venue,
            article.Publication.Year,
            article.Publication.Doi?.Value,
            article.Publication.Url?.ToString(),
            article.PdfUrl?.ToString(),
            article.Language,
            article.CitationCount,
            article.PrimarySource,
            article.StudyType,
            article.Topics.Select(t => t.Value).ToList());
    }

    public static Article ToArticle(this AcademicArticleCandidate candidate)
    {
        var publication = PublicationInfo.Create(
            candidate.Venue,
            candidate.Year,
            candidate.Doi,
            candidate.Url);

        var authors = candidate.Authors
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(name => Author.Create(name))
            .ToList();

        var externalRef = ExternalReference.Create(
            candidate.PrimarySource,
            candidate.ExternalId);

        return Article.Create(
            title: candidate.Title,
            primarySource: candidate.PrimarySource,
            publication: publication,
            externalReferences: [externalRef],
            authors: authors,
            abstractText: candidate.Abstract,
            citationCount: candidate.CitationCount,
            language: candidate.Language,
            studyType: candidate.StudyType,
            pdfUrl: candidate.PdfUrl);
    }
}
