using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.ArticleIdConverter);

        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Abstract);
        builder.Property(x => x.Language).HasMaxLength(16);
        builder.Property(x => x.CitationCount);
        builder.Property(x => x.PrimarySource).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.StudyType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.PdfUrl).HasConversion(TypedIdConversions.UriConverter).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.UpdatedAtUtc);

        builder.OwnsOne(x => x.Publication, publication =>
        {
            publication.Property(p => p.Venue).HasMaxLength(300).HasColumnName("Venue");
            publication.Property(p => p.Year).HasColumnName("Year");
            publication.Property(p => p.Doi)
                .HasConversion(TypedIdConversions.DoiConverter)
                .HasMaxLength(200)
                .HasColumnName("Doi");
            publication.Property(p => p.Url)
                .HasConversion(TypedIdConversions.UriConverter)
                .HasMaxLength(1000)
                .HasColumnName("Url");

            publication.HasIndex(p => p.Doi);
        });

        builder.Navigation(x => x.Publication).IsRequired();

        builder.OwnsMany<Author>("_authors", authors =>
        {
            authors.ToTable("ArticleAuthors");
            authors.WithOwner().HasForeignKey("ArticleId");
            authors.Property<int>("Id");
            authors.HasKey("Id");
            authors.Property(a => a.Name).HasMaxLength(200).IsRequired();
            authors.Property(a => a.Orcid).HasMaxLength(50);
            authors.Property(a => a.Affiliation).HasMaxLength(300);
        });

        builder.OwnsMany<ExternalReference>("_externalReferences", references =>
        {
            references.ToTable("ArticleExternalReferences");
            references.WithOwner().HasForeignKey("ArticleId");
            references.Property<int>("Id");
            references.HasKey("Id");
            references.Property(r => r.System).HasConversion<string>().HasMaxLength(32);
            references.Property(r => r.ExternalId).HasMaxLength(200).IsRequired();
            references.HasIndex(r => new { r.System, r.ExternalId });
        });

        builder.OwnsMany<TopicTag>("_topics", topics =>
        {
            topics.ToTable("ArticleTopics");
            topics.WithOwner().HasForeignKey("ArticleId");
            topics.Property<int>("Id");
            topics.HasKey("Id");
            topics.Property(t => t.Value).HasMaxLength(100).IsRequired();
        });

        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Authors);
        builder.Ignore(x => x.ExternalReferences);
        builder.Ignore(x => x.Topics);
    }
}
