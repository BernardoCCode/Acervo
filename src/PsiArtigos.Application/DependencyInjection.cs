using Microsoft.Extensions.DependencyInjection;
using PsiArtigos.Application.Services;

namespace PsiArtigos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ArticleService>();
        services.AddScoped<SearchService>();
        services.AddScoped<LibraryService>();
        services.AddScoped<LearningTrailService>();
        services.AddScoped<InsightService>();
        services.AddScoped<ReadingService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<RecommendationEngineService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
