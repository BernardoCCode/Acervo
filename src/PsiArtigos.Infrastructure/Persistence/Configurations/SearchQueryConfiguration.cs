using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.ValueObjects;
using PsiArtigos.Infrastructure.Persistence.Conversions;

namespace PsiArtigos.Infrastructure.Persistence.Configurations;

internal sealed class SearchQueryConfiguration : IEntityTypeConfiguration<SearchQuery>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public void Configure(EntityTypeBuilder<SearchQuery> builder)
    {
        builder.ToTable("SearchQueries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(TypedIdConversions.SearchQueryIdConverter);
        builder.Property(x => x.UserId).HasConversion(TypedIdConversions.NullableUserIdConverter);
        builder.Property(x => x.RawText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ResultCount);
        builder.Property(x => x.ExecutedAtUtc);
        builder.Property(x => x.LastAccessedAtUtc);
        builder.HasIndex(x => new { x.UserId, x.LastAccessedAtUtc });

        builder.OwnsOne(x => x.Filters, filters =>
        {
            filters.Property(f => f.YearMin).HasColumnName("FilterYearMin");
            filters.Property(f => f.YearMax).HasColumnName("FilterYearMax");
            filters.Property(f => f.Language).HasMaxLength(16).HasColumnName("FilterLanguage");
            filters.Property(f => f.StudyType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasColumnName("FilterStudyType");
            filters.Property(f => f.MinCitations).HasColumnName("FilterMinCitations");

            var sourcesConverter = new ValueConverter<List<SourceSystem>, string>(
                sources => JsonSerializer.Serialize(sources, JsonOptions),
                json => JsonSerializer.Deserialize<List<SourceSystem>>(json, JsonOptions) ?? new List<SourceSystem>());

            var sourcesComparer = new ValueComparer<List<SourceSystem>>(
                (left, right) => SequenceEqual(left, right),
                list => GetHash(list),
                list => list == null ? new List<SourceSystem>() : list.ToList());

            filters.Property<List<SourceSystem>>("_sources")
                .HasColumnName("FilterSourcesJson")
                .HasConversion(sourcesConverter)
                .Metadata.SetValueComparer(sourcesComparer);
        });

        builder.Navigation(x => x.Filters).IsRequired();

        var resultIdsConverter = new ValueConverter<List<ArticleId>, string>(
            ids => JsonSerializer.Serialize(ids.Select(id => id.Value).ToList(), JsonOptions),
            json => (JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions) ?? new List<Guid>())
                .Select(ArticleId.From)
                .ToList());

        var resultIdsComparer = new ValueComparer<List<ArticleId>>(
            (left, right) => SequenceEqual(left, right),
            list => GetHash(list),
            list => list == null ? new List<ArticleId>() : list.ToList());

        builder.Property<List<ArticleId>>("_resultArticleIds")
            .HasField("_resultArticleIds")
            .HasColumnName("ResultArticleIdsJson")
            .HasConversion(resultIdsConverter)
            .Metadata.SetValueComparer(resultIdsComparer);

        builder.Ignore(x => x.ResultArticleIds);
        builder.Ignore(x => x.DomainEvents);
    }

    private static bool SequenceEqual<T>(List<T>? left, List<T>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.SequenceEqual(right);
    }

    private static int GetHash<T>(List<T>? list)
    {
        if (list is null)
            return 0;

        var hash = 0;
        foreach (var item in list)
            hash = HashCode.Combine(hash, item);

        return hash;
    }
}
