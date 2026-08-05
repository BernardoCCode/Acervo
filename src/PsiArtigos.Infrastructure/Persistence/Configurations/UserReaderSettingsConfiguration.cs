using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class UserReaderSettingsConfiguration : IEntityTypeConfiguration<UserReaderSettings>
{
    public void Configure(EntityTypeBuilder<UserReaderSettings> builder)
    {
        builder.ToTable("UserReaderSettings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.UpdatedAtUtc);

        builder.OwnsOne(x => x.Preferences, preferences =>
        {
            preferences.Property(p => p.DarkMode).HasColumnName("DarkMode");
            preferences.Property(p => p.FontSize).HasColumnName("FontSize");
            preferences.Property(p => p.PreferredTranslationLanguage)
                .HasMaxLength(16)
                .HasColumnName("PreferredTranslationLanguage");
        });

        builder.Navigation(x => x.Preferences).IsRequired();
        builder.Ignore(x => x.DomainEvents);
    }
}
