using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PsiArtigos.Infrastructure.External.Content;

public sealed class ReadableContentExtractor : IReadableContentExtractor
{
    private readonly HttpClient _httpClient;
    private static readonly HtmlParser HtmlParser = new();

    public ReadableContentExtractor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ExtractedReadableContent?> FromPdfAsync(
        byte[] pdfBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pdfBytes.Length < 100)
            return Task.FromResult<ExtractedReadableContent?>(null);

        try
        {
            using var document = PdfDocument.Open(pdfBytes);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var words = page.GetWords()
                    .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                    .ToList();
                if (words.Count == 0)
                    continue;

                var pageText = AssemblePageText(words);
                if (string.IsNullOrWhiteSpace(pageText))
                    continue;

                if (builder.Length > 0)
                    builder.Append("\n\n");

                builder.Append(CleanupText(pageText));
            }

            var body = builder.ToString().Trim();
            if (body.Length < 80 || LooksLikeGibberish(body))
                return Task.FromResult<ExtractedReadableContent?>(null);

            return Task.FromResult<ExtractedReadableContent?>(
                new ExtractedReadableContent(body, ReadableContentSource.PdfText, document.NumberOfPages));
        }
        catch
        {
            return Task.FromResult<ExtractedReadableContent?>(null);
        }
    }

    /// <summary>
    /// Rebuilds page text respecting a two-column layout when one is detected.
    /// Naive document-order word joining interleaves the columns line by line,
    /// which is what produced the mixed-language garbled articles.
    /// </summary>
    private static string AssemblePageText(List<Word> words)
    {
        if (words.Count < 40)
            return BuildColumnText(words);

        var minLeft = words.Min(w => w.BoundingBox.Left);
        var maxRight = words.Max(w => w.BoundingBox.Right);
        var width = maxRight - minLeft;
        var mid = (minLeft + maxRight) / 2.0;

        // Two-column pages have an empty vertical gutter at the center: almost no
        // word touches a narrow band around the midline. Single-column justified
        // text crosses the center on nearly every line, so this rarely misfires.
        var band = width * 0.015;
        var touchingBand = words.Count(w =>
            w.BoundingBox.Right > mid - band && w.BoundingBox.Left < mid + band);

        var left = words.Where(w => Center(w) < mid).ToList();
        var right = words.Where(w => Center(w) >= mid).ToList();

        var twoColumns =
            touchingBand <= words.Count * 0.02
            && left.Count > words.Count * 0.25
            && right.Count > words.Count * 0.25;

        if (!twoColumns)
            return BuildColumnText(words);

        var leftText = BuildColumnText(left);
        var rightText = BuildColumnText(right);
        return string.IsNullOrWhiteSpace(rightText)
            ? leftText
            : $"{leftText}\n\n{rightText}";
    }

    private static double Center(Word w)
        => (w.BoundingBox.Left + w.BoundingBox.Right) / 2.0;

    /// <summary>Orders words into lines (top→bottom, left→right), detects paragraph
    /// gaps, and merges end-of-line hyphenation.</summary>
    private static string BuildColumnText(List<Word> words)
    {
        if (words.Count == 0)
            return string.Empty;

        var medianHeight = Median(words.Select(w => w.BoundingBox.Height));
        var lineTolerance = Math.Max(medianHeight * 0.6, 2.0);

        var lines = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(w => w.BoundingBox.Bottom))
        {
            var current = lines.LastOrDefault();
            if (current is not null
                && Math.Abs(current[0].BoundingBox.Bottom - word.BoundingBox.Bottom)
                    <= lineTolerance)
            {
                current.Add(word);
            }
            else
            {
                lines.Add([word]);
            }
        }

        foreach (var line in lines)
            line.Sort((a, b) => a.BoundingBox.Left.CompareTo(b.BoundingBox.Left));

        // Paragraph breaks come from unusually large vertical gaps between lines.
        var gaps = new List<double>();
        for (var i = 1; i < lines.Count; i++)
        {
            gaps.Add(lines[i - 1][0].BoundingBox.Bottom - lines[i][0].BoundingBox.Top);
        }
        var medianGap = gaps.Count > 0 ? Median(gaps) : 0;

        var sb = new StringBuilder();
        for (var i = 0; i < lines.Count; i++)
        {
            var lineText = string.Join(' ', lines[i].Select(w => w.Text.Trim()));
            if (string.IsNullOrWhiteSpace(lineText))
                continue;

            if (sb.Length == 0)
            {
                sb.Append(lineText);
                continue;
            }

            var gap = lines[i - 1][0].BoundingBox.Bottom - lines[i][0].BoundingBox.Top;
            var isParagraphBreak = medianGap > 0 && gap > medianGap * 1.9 + 1.0;

            if (isParagraphBreak)
            {
                sb.Append("\n\n").Append(lineText);
            }
            else if (sb[^1] == '-' && lineText.Length > 0 && char.IsLower(lineText[0]))
            {
                // De-hyphenate words broken across lines ("compu-\ntacional").
                sb.Length--;
                sb.Append(lineText);
            }
            else
            {
                sb.Append(' ').Append(lineText);
            }
        }

        return sb.ToString();
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count == 0)
            return 0;
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2.0 : sorted[mid];
    }

    /// <summary>Rejects extractions that came out unreadable (broken encodings,
    /// symbol soup, or shredded words) so the reader never shows garbage.</summary>
    private static bool LooksLikeGibberish(string body)
    {
        var tokens = body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var alphaTokens = tokens.Where(t => t.Any(char.IsLetter)).ToList();
        if (alphaTokens.Count < 60)
            return false; // too short to judge — let the length gate decide

        var singleChar = alphaTokens.Count(t => t.Length == 1);
        var avgLength = alphaTokens.Average(t => (double)t.Length);
        var letterRatio =
            body.Count(char.IsLetter) / (double)Math.Max(1, body.Length);

        return singleChar / (double)alphaTokens.Count > 0.25
            || avgLength < 2.8
            || letterRatio < 0.45;
    }

    public async Task<ExtractedReadableContent?> FromHtmlUrlAsync(
        string pageUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(pageUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        try
        {
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (!mediaType.Contains("html", StringComparison.OrdinalIgnoreCase)
                && !mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)
                && mediaType.Length > 0)
            {
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(html) || html.Length < 200)
                return null;

            var document = await HtmlParser.ParseDocumentAsync(html, cancellationToken);
            foreach (var node in document.QuerySelectorAll("script, style, nav, footer, header, noscript, svg, form"))
                node.Remove();

            var main = document.QuerySelector("article, main, .abstract, #content, .content")
                ?? document.Body;

            var text = main?.TextContent ?? string.Empty;
            text = CleanupText(WebUtility.HtmlDecode(text));

            if (text.Length < 200)
                return null;

            // Keep a bounded extract for HTML pages (landing pages are noisy)
            if (text.Length > 40_000)
                text = text[..40_000].TrimEnd() + "…";

            return new ExtractedReadableContent(text, ReadableContentSource.HtmlPage, null);
        }
        catch
        {
            return null;
        }
    }

    private static string CleanupText(string text)
    {
        // Postgres rejects U+0000 in UTF-8 text columns.
        var normalized = text
            .Replace("\0", string.Empty, StringComparison.Ordinal)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\u00a0', ' ');

        normalized = Regex.Replace(normalized, "[ \t]+", " ");
        normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");
        return normalized.Trim();
    }
}
