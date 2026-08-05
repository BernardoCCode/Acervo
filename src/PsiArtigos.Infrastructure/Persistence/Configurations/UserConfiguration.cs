using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.UpdatedAtUtc);

        builder.OwnsOne(x => x.Profile, profile =>
        {
            profile.Property(p => p.DisplayName).HasMaxLength(200).HasColumnName("DisplayName");
            profile.Property(p => p.PreferredLanguage).HasMaxLength(16).HasColumnName("PreferredLanguage");

            profile.OwnsMany<TopicTag>("_interests", interests =>
            {
                interests.ToTable("UserInterests");
                interests.WithOwner().HasForeignKey("UserId");
                interests.Property<int>("Id");
                interests.HasKey("Id");
                interests.Property(t => t.Value).HasMaxLength(100).IsRequired();
            });

            profile.Ignore(p => p.Interests);
        });

        builder.Navigation(x => x.Profile).IsRequired();
        builder.Ignore(x => x.DomainEvents);
    }
}
