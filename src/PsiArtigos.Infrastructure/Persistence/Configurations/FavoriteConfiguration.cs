using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.FavoriteIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.ArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.CreatedAtUtc);

        builder.HasIndex(x => new { x.UserId, x.ArticleId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
