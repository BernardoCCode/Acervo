using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PsiArtigos.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<PsiArtigosDbContext>();

        try
        {
            var raw = dbContext.Database.GetConnectionString() ?? "(null)";
            var host = "(unknown)";
            try
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(raw);
                host = $"{builder.Host}/{builder.Database} user={builder.Username}";
            }
            catch
            {
                /* ignore parse errors for logging */
            }

            logger.LogInformation("Applying database migrations to {Target}...", host);
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database is up to date.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to migrate database.");
            throw;
        }
    }
}
