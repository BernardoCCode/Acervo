using PsiArtigos.Application.Interfaces;

namespace PsiArtigos.Infrastructure.External.Pdf;

public sealed class PdfFetchService : IPdfFetchPort
{
    private readonly HttpClient _httpClient;

    public PdfFetchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PdfDocumentContent?> FetchAsync(
        string pdfUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfUrl))
            return null;

        if (!Uri.TryCreate(pdfUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        using var response = await _httpClient.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.Length == 0)
            return null;

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrWhiteSpace(contentType)
            || contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
        {
            contentType = "application/pdf";
        }

        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? Path.GetFileName(uri.AbsolutePath);

        if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.', StringComparison.Ordinal))
            fileName = "article.pdf";

        return new PdfDocumentContent(bytes, contentType, fileName);
    }
}
