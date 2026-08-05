using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class ArticleContentConfiguration : IEntityTypeConfiguration<ArticleContent>
{
    public void Configure(EntityTypeBuilder<ArticleContent> builder)
    {
        builder.ToTable("ArticleContents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.ArticleIdConverter);

        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PageCount);
        builder.Property(x => x.ExtractedAtUtc);

        builder.Ignore(x => x.DomainEvents);
    }
}
