using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PsiArtigos.Application.DTOs.Search;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Infrastructure.External.AcademicSearch;

namespace PsiArtigos.Infrastructure.Caching;

public sealed class CachingAcademicSearchService : IAcademicSearchPort
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AcademicSearchService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingAcademicSearchService> _logger;

    public CachingAcademicSearchService(
        AcademicSearchService inner,
        IDistributedCache cache,
        ILogger<CachingAcademicSearchService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AcademicArticleCandidate>> SearchAsync(
        string query,
        SearchFiltersRequest? filters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildKey(query, filters);

        try
        {
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                var hit = JsonSerializer.Deserialize<List<AcademicArticleCandidate>>(cached, JsonOptions);
                if (hit is not null)
                    return hit;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Search cache read failed for key {CacheKey}", cacheKey);
        }

        var results = await _inner.SearchAsync(query, filters, cancellationToken);

        try
        {
            var payload = JsonSerializer.Serialize(results, JsonOptions);
            await _cache.SetStringAsync(
                cacheKey,
                payload,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Search cache write failed for key {CacheKey}", cacheKey);
        }

        return results;
    }

    private static string BuildKey(string query, SearchFiltersRequest? filters)
    {
        var normalized = (query ?? string.Empty).Trim().ToLowerInvariant();
        var filterPart = filters is null
            ? string.Empty
            : JsonSerializer.Serialize(filters, JsonOptions);
        var raw = normalized + "|" + filterPart;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        return $"search:v1:{hash}";
    }
}
