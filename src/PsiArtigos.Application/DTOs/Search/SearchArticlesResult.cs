using PsiArtigos.Application.DTOs.Articles;

namespace PsiArtigos.Application.DTOs.Search;

public sealed record SearchArticlesResult(
    Guid SearchQueryId,
    string Query,
    int ResultCount,
    IReadOnlyList<ArticleDto> Articles);
