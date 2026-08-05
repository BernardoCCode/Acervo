using Microsoft.EntityFrameworkCore;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Entities;

namespace PsiArtigos.Infrastructure.Persistence;

public sealed class PsiArtigosDbContext : DbContext
{
    public PsiArtigosDbContext(DbContextOptions<PsiArtigosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleContent> ArticleContents => Set<ArticleContent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<LearningTrail> LearningTrails => Set<LearningTrail>();
    public DbSet<GeneratedInsight> GeneratedInsights => Set<GeneratedInsight>();
    public DbSet<ReadingSession> ReadingSessions => Set<ReadingSession>();
    public DbSet<UserReaderSettings> UserReaderSettings => Set<UserReaderSettings>();
    public DbSet<SearchQuery> SearchQueries => Set<SearchQuery>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<CitationLink> CitationLinks => Set<CitationLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PsiArtigosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
