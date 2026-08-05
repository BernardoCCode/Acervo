using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class CitationLinkConfiguration : IEntityTypeConfiguration<CitationLink>
{
    public void Configure(EntityTypeBuilder<CitationLink> builder)
    {
        builder.ToTable("CitationLinks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => CitationLinkId.From(value));

        builder.Property(x => x.FromArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.ToArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.LinkType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedAtUtc);

        builder.HasIndex(x => new { x.FromArticleId, x.ToArticleId, x.LinkType }).IsUnique();
    }
}
