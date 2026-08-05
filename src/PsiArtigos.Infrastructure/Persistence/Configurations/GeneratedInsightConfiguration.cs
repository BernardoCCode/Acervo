using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class GeneratedInsightConfiguration : IEntityTypeConfiguration<GeneratedInsight>
{
    public void Configure(EntityTypeBuilder<GeneratedInsight> builder)
    {
        builder.ToTable("GeneratedInsights");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.InsightIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.ArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.SourceLanguage).HasMaxLength(16);
        builder.Property(x => x.TargetLanguage).HasMaxLength(16);
        builder.Property(x => x.CreatedAtUtc);

        builder.HasIndex(x => new { x.UserId, x.ArticleId, x.Type });
        builder.Ignore(x => x.DomainEvents);
    }
}
