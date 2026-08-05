using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class ReadingSessionConfiguration : IEntityTypeConfiguration<ReadingSession>
{
    public void Configure(EntityTypeBuilder<ReadingSession> builder)
    {
        builder.ToTable("ReadingSessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.ReadingSessionIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.ArticleId).HasConversion(TypedIdConversions.ArticleIdConverter);
        builder.Property(x => x.LastOpenedAtUtc);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.IsCompleted);
        builder.Property(x => x.OpenCount);
        builder.Property(x => x.ActiveReadingSeconds);

        builder.OwnsOne(x => x.Progress, progress =>
        {
            progress.Property(p => p.Percent).HasColumnName("ProgressPercent");
            progress.Property(p => p.PageNumber).HasColumnName("ProgressPageNumber");
            progress.Property(p => p.CharacterOffset).HasColumnName("ProgressCharacterOffset");
            progress.Ignore(p => p.IsCompleted);
        });

        builder.Navigation(x => x.Progress).IsRequired();

        builder.OwnsMany<Highlight>("_highlights", highlights =>
        {
            highlights.ToTable("Highlights");
            highlights.WithOwner().HasForeignKey("ReadingSessionId");
            highlights.HasKey(h => h.Id);
            highlights.Property(h => h.Id).HasConversion(TypedIdConversions.HighlightIdConverter);
            highlights.Property(h => h.QuotedText).IsRequired();
            highlights.Property(h => h.Color).HasConversion<string>().HasMaxLength(32);
            highlights.Property(h => h.CreatedAtUtc);

            highlights.OwnsOne(h => h.Range, range =>
            {
                range.Property(r => r.StartOffset).HasColumnName("StartOffset");
                range.Property(r => r.EndOffset).HasColumnName("EndOffset");
                range.Property(r => r.PageNumber).HasColumnName("PageNumber");
            });

            highlights.Navigation(h => h.Range).IsRequired();

            highlights.OwnsMany<Annotation>("_annotations", annotations =>
            {
                annotations.ToTable("Annotations");
                annotations.WithOwner().HasForeignKey("HighlightId");
                annotations.HasKey(a => a.Id);
                annotations.Property(a => a.Id).HasConversion(TypedIdConversions.AnnotationIdConverter);
                annotations.Property(a => a.HighlightId).HasConversion(TypedIdConversions.HighlightIdConverter);
                annotations.Property(a => a.Note).IsRequired();
                annotations.Property(a => a.CreatedAtUtc);
                annotations.Property(a => a.UpdatedAtUtc);
            });

            highlights.Ignore(h => h.Annotations);
        });

        builder.Ignore(x => x.Highlights);
        builder.HasIndex(x => new { x.UserId, x.ArticleId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
