using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Services;
using PsiArtigos.Infrastructure.Caching;
using PsiArtigos.Infrastructure.External.AcademicSearch;
using PsiArtigos.Infrastructure.External.AI;
using PsiArtigos.Infrastructure.External.Content;
using PsiArtigos.Infrastructure.External.Pdf;
using PsiArtigos.Infrastructure.Identity;
using PsiArtigos.Infrastructure.Options;
using PsiArtigos.Infrastructure.Persistence;
using PsiArtigos.Infrastructure.Persistence.Repositories;
using PsiArtigos.Infrastructure.Services;

namespace PsiArtigos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AcademicSearchOptions>(
            configuration.GetSection(AcademicSearchOptions.SectionName));
        services.Configure<AiOptions>(
            configuration.GetSection(AiOptions.SectionName));
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "acervo:";
            });
        }
        else
        {
            // In-process fallback — no Redis required for local/dev.
            services.AddDistributedMemoryCache();
        }

        services.AddMemoryCache();

        services.AddDbContext<PsiArtigosDbContext>(options =>
        {
            var connectionString = NormalizePostgresConnectionString(
                configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=psiartigos;Username=psiartigos;Password=psiartigos");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleContentRepository, ArticleContentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<ILearningTrailRepository, LearningTrailRepository>();
        services.AddScoped<IGeneratedInsightRepository, GeneratedInsightRepository>();
        services.AddScoped<IReadingSessionRepository, ReadingSessionRepository>();
        services.AddScoped<IUserReaderSettingsRepository, UserReaderSettingsRepository>();
        services.AddScoped<ISearchQueryRepository, SearchQueryRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHashPort, PasswordHashService>();
        services.AddSingleton<IAccessTokenPort, JwtAccessTokenService>();
        services.AddSingleton<ICitationFormatter, CitationFormatter>();

        services.AddHttpClient<OpenAlexSearchClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openalex.org/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Acervo/1.0 (mailto:dev@acervo.local)");
        });

        services.AddHttpClient<ArxivSearchClient>(client =>
        {
            client.BaseAddress = new Uri("https://export.arxiv.org/");
            // arXiv often 503s; don't block the whole search on it.
            client.Timeout = TimeSpan.FromSeconds(8);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Acervo/1.0");
        });

        services.AddHttpClient<CrossrefSearchClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.crossref.org/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Acervo/1.0 (mailto:dev@acervo.local)");
        });

        services.AddHttpClient<EuropePmcSearchClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.ebi.ac.uk/europepmc/webservices/rest/");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Acervo/1.0 (mailto:dev@acervo.local)");
        });

        services.AddHttpClient<SemanticScholarSearchClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.semanticscholar.org/graph/v1/");
            client.Timeout = TimeSpan.FromSeconds(12);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Acervo/1.0 (mailto:dev@acervo.local)");
        });

        services.AddScoped<AcademicSearchService>();
        services.AddScoped<IAcademicSearchPort, CachingAcademicSearchService>();

        services.AddHttpClient<PdfFetchService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Acervo/1.0 (mailto:dev@acervo.local)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/pdf,*/*");
        });
        services.AddScoped<IPdfFetchPort, CachingPdfFetchService>();

        services.AddHttpClient<IReadableContentExtractor, ReadableContentExtractor>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(1);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Acervo/1.0 (mailto:dev@acervo.local)");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,*/*");
        });

        // Real AI when a key is configured; deterministic local fallback otherwise.
        var aiOptions = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>();
        if (!string.IsNullOrWhiteSpace(aiOptions?.ApiKey))
        {
            services.AddHttpClient<IAiInsightPort, OpenAiInsightService>(client =>
            {
                var baseUrl = string.IsNullOrWhiteSpace(aiOptions.BaseUrl)
                    ? "https://api.openai.com/v1/"
                    : aiOptions.BaseUrl.EndsWith('/') ? aiOptions.BaseUrl : aiOptions.BaseUrl + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            services.AddHttpClient<IAiLearningPort, OpenAiLearningService>(client =>
            {
                var baseUrl = string.IsNullOrWhiteSpace(aiOptions.BaseUrl)
                    ? "https://api.openai.com/v1/"
                    : aiOptions.BaseUrl.EndsWith('/') ? aiOptions.BaseUrl : aiOptions.BaseUrl + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(90);
            });
            services.AddHttpClient<IAiRecommendationPort, OpenAiRecommendationService>(client =>
            {
                var baseUrl = string.IsNullOrWhiteSpace(aiOptions.BaseUrl)
                    ? "https://api.openai.com/v1/"
                    : aiOptions.BaseUrl.EndsWith('/') ? aiOptions.BaseUrl : aiOptions.BaseUrl + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(60);
            });
        }
        else
        {
            services.AddScoped<IAiInsightPort, LocalAiInsightService>();
            services.AddScoped<IAiLearningPort, LocalAiLearningService>();
            services.AddScoped<IAiRecommendationPort, LocalAiRecommendationService>();
        }

        return services;
    }

    /// <summary>
    /// Neon connection strings often include channel_binding, which can crash Npgsql on boot.
    /// Normalize to a key/value string Npgsql accepts reliably in production.
    /// </summary>
    private static string NormalizePostgresConnectionString(string connectionString)
    {
        var cleaned = connectionString.Trim().Trim('"', '\'');
        cleaned = cleaned
            .Replace("&channel_binding=require", "", StringComparison.OrdinalIgnoreCase)
            .Replace("?channel_binding=require&", "?", StringComparison.OrdinalIgnoreCase)
            .Replace("?channel_binding=require", "", StringComparison.OrdinalIgnoreCase)
            .Replace(";Channel Binding=Require", "", StringComparison.OrdinalIgnoreCase)
            .Replace(";Channel Binding=Prefer", "", StringComparison.OrdinalIgnoreCase)
            .TrimEnd('?', '&');

        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(cleaned)
            {
                ChannelBinding = Npgsql.ChannelBinding.Disable,
            };

            if (builder.Host?.Contains("neon.tech", StringComparison.OrdinalIgnoreCase) == true)
            {
                builder.SslMode = Npgsql.SslMode.Require;
                builder.TrustServerCertificate = true;
            }

            return builder.ConnectionString;
        }
        catch
        {
            return cleaned;
        }
    }
}
