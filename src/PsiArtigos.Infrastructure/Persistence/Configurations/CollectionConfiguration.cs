using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.CollectionIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.UpdatedAtUtc);

        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();

        builder.OwnsMany<CollectionItem>("_items", items =>
        {
            items.ToTable("CollectionItems");
            items.WithOwner().HasForeignKey("CollectionId");
            items.Property<int>("Id");
            items.HasKey("Id");
            items.Property(i => i.ArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
            items.Property(i => i.AddedAtUtc);
            items.HasIndex(i => i.ArticleId);
        });

        builder.Ignore(x => x.Items);
        builder.Ignore(x => x.DomainEvents);
    }
}
