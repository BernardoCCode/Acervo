using System.Text;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Services;

namespace PsiArtigos.Infrastructure.Services;

public sealed class CitationFormatter : ICitationFormatter
{
    public string Format(Article article, CitationStyle style)
        => style switch
        {
            CitationStyle.BibTeX => FormatBibTeX(article),
            CitationStyle.Apa => FormatApa(article),
            CitationStyle.Abnt => FormatAbnt(article),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };

    private static string FormatBibTeX(Article article)
    {
        var key = BuildKey(article);
        var authors = string.Join(" and ", article.Authors.Select(a => a.Name));
        var sb = new StringBuilder();
        sb.AppendLine($"@article{{{key},");
        sb.AppendLine($"  title = {{{article.Title}}},");
        if (!string.IsNullOrWhiteSpace(authors))
            sb.AppendLine($"  author = {{{authors}}},");
        if (!string.IsNullOrWhiteSpace(article.Publication.Venue))
            sb.AppendLine($"  journal = {{{article.Publication.Venue}}},");
        if (article.Publication.Year is int year)
            sb.AppendLine($"  year = {{{year}}},");
        if (article.Publication.Doi is not null)
            sb.AppendLine($"  doi = {{{article.Publication.Doi.Value}}},");
        sb.Append('}');
        return sb.ToString();
    }

    private static string FormatApa(Article article)
    {
        var authors = FormatApaAuthors(article.Authors.Select(a => a.Name).ToList());
        var year = article.Publication.Year?.ToString() ?? "n.d.";
        var venue = article.Publication.Venue;
        var doi = article.Publication.Doi is null
            ? string.Empty
            : $" https://doi.org/{article.Publication.Doi.Value}";

        return string.IsNullOrWhiteSpace(venue)
            ? $"{authors} ({year}). {article.Title}.{doi}"
            : $"{authors} ({year}). {article.Title}. {venue}.{doi}";
    }

    private static string FormatAbnt(Article article)
    {
        var authors = FormatAbntAuthors(article.Authors.Select(a => a.Name).ToList());
        var year = article.Publication.Year?.ToString() ?? "s.d.";
        var venue = article.Publication.Venue;
        var doi = article.Publication.Doi is null
            ? string.Empty
            : $" DOI: {article.Publication.Doi.Value}.";

        return string.IsNullOrWhiteSpace(venue)
            ? $"{authors} {article.Title}. {year}.{doi}"
            : $"{authors} {article.Title}. {venue}, {year}.{doi}";
    }

    private static string FormatApaAuthors(IReadOnlyList<string> authors)
    {
        if (authors.Count == 0)
            return "Unknown";

        if (authors.Count == 1)
            return authors[0];

        if (authors.Count == 2)
            return $"{authors[0]} & {authors[1]}";

        return $"{string.Join(", ", authors.Take(authors.Count - 1))}, & {authors[^1]}";
    }

    private static string FormatAbntAuthors(IReadOnlyList<string> authors)
    {
        if (authors.Count == 0)
            return "UNKNOWN.";

        return string.Join("; ", authors.Select(ToAbntName)) + ".";
    }

    private static string ToAbntName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].ToUpperInvariant();

        var last = parts[^1].ToUpperInvariant();
        var given = string.Join(' ', parts.Take(parts.Length - 1));
        return $"{last}, {given}";
    }

    private static string BuildKey(Article article)
    {
        var author = article.Authors.FirstOrDefault()?.Name?.Split(' ').LastOrDefault() ?? "article";
        var year = article.Publication.Year?.ToString() ?? "nodate";
        return $"{author}{year}".ToLowerInvariant();
    }
}
