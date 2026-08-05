using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.RecommendationIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.ArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.Reason).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.Score);
        builder.Property(x => x.Explanation).HasMaxLength(1000);
        builder.Property(x => x.SourceArticleId).HasConversion(TypedIdConversions.NullableArticleIdConverter);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.ExpiresAtUtc);
        builder.Property(x => x.TopicScore);
        builder.Property(x => x.EngagementScore);
        builder.Property(x => x.QualityScore);
        builder.Property(x => x.FreshnessScore);
        builder.Property(x => x.IsDismissed);

        builder.HasIndex(x => new { x.UserId, x.IsDismissed });
        builder.Ignore(x => x.DomainEvents);
    }
}
