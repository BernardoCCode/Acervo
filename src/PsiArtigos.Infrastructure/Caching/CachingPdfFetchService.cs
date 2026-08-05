using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Infrastructure.External.Pdf;

namespace PsiArtigos.Infrastructure.Caching;

public sealed class CachingPdfFetchService : IPdfFetchPort
{
    private const int MaxCachedBytes = 5 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly PdfFetchService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingPdfFetchService> _logger;

    public CachingPdfFetchService(
        PdfFetchService inner,
        IDistributedCache cache,
        ILogger<CachingPdfFetchService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PdfDocumentContent?> FetchAsync(
        string pdfUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pdfUrl))
            return null;

        var cacheKey = BuildKey(pdfUrl);

        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                var entry = JsonSerializer.Deserialize<CachedPdf>(cached, JsonOptions);
                if (entry?.BytesBase64 is not null)
                {
                    return new PdfDocumentContent(
                        Convert.FromBase64String(entry.BytesBase64),
                        entry.ContentType,
                        entry.FileName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PDF cache read failed for {PdfUrl}", pdfUrl);
        }

        var content = await _inner.FetchAsync(pdfUrl, cancellationToken);
        if (content is null || content.Bytes.Length == 0 || content.Bytes.Length > MaxCachedBytes)
            return content;

        try
        {
            var payload = JsonSerializer.Serialize(
                new CachedPdf(
                    Convert.ToBase64String(content.Bytes),
                    content.ContentType,
                    content.FileName ?? "article.pdf"),
                JsonOptions);

            await _cache.SetStringAsync(
                cacheKey,
                payload,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PDF cache write failed for {PdfUrl}", pdfUrl);
        }

        return content;
    }

    private static string BuildKey(string pdfUrl)
    {
        var normalized = pdfUrl.Trim().ToLowerInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
        return $"pdf:v1:{hash}";
    }

    private sealed record CachedPdf(string BytesBase64, string ContentType, string FileName);
}
