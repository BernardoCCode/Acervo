using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class LearningTrailConfiguration : IEntityTypeConfiguration<LearningTrail>
{
    public void Configure(EntityTypeBuilder<LearningTrail> builder)
    {
        builder.ToTable("LearningTrails");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.LearningTrailIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.UserIdConverter);
        builder.Property(x => x.Prompt).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Topic).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc);
        builder.Property(x => x.UpdatedAtUtc);

        builder.OwnsMany<TrailStep>("_steps", steps =>
        {
            steps.ToTable("LearningTrailSteps");
            steps.WithOwner().HasForeignKey("LearningTrailId");
            steps.HasKey(s => s.Id);
            steps.Property(s => s.Id).HasConversion(TypedIdConversions.TrailStepIdConverter);
            steps.Property(s => s.Order);
            steps.Property(s => s.Title).HasMaxLength(300).IsRequired();
            steps.Property(s => s.Difficulty).HasConversion<string>().HasMaxLength(32);
            steps.Property(s => s.ArticleId).HasConversion(TypedIdConversions.NullableArticleIdConverter);
            steps.Property(s => s.Rationale).HasMaxLength(1000);
            steps.Ignore(s => s.HasArticle);
        });

        builder.Ignore(x => x.Steps);
        builder.Ignore(x => x.DomainEvents);
    }
}
