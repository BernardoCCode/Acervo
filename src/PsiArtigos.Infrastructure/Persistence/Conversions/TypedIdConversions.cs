using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Infrastructure.Persistence.Conversions;

internal static class TypedIdConversions
{
    public static ValueConverter<ArticleId, Guid> ArticleIdConverter { get; } = new(
        id => id.Value,
        value => ArticleId.From(value));

    public static ValueConverter<ArticleId?, Guid?> NullableArticleIdConverter { get; } = new(
        id => id.HasValue ? id.Value.Value : null,
        value => value.HasValue ? ArticleId.From(value.Value) : null);

    public static ValueConverter<UserId, Guid> UserIdConverter { get; } = new(
        id => id.Value,
        value => UserId.From(value));

    public static ValueConverter<UserId?, Guid?> NullableUserIdConverter { get; } = new(
        id => id.HasValue ? id.Value.Value : null,
        value => value.HasValue ? UserId.From(value.Value) : null);

    public static ValueConverter<CollectionId, Guid> CollectionIdConverter { get; } = new(
        id => id.Value,
        value => CollectionId.From(value));

    public static ValueConverter<FavoriteId, Guid> FavoriteIdConverter { get; } = new(
        id => id.Value,
        value => FavoriteId.From(value));

    public static ValueConverter<LearningTrailId, Guid> LearningTrailIdConverter { get; } = new(
        id => id.Value,
        value => LearningTrailId.From(value));

    public static ValueConverter<TrailStepId, Guid> TrailStepIdConverter { get; } = new(
        id => id.Value,
        value => TrailStepId.From(value));

    public static ValueConverter<ReadingSessionId, Guid> ReadingSessionIdConverter { get; } = new(
        id => id.Value,
        value => ReadingSessionId.From(value));

    public static ValueConverter<HighlightId, Guid> HighlightIdConverter { get; } = new(
        id => id.Value,
        value => HighlightId.From(value));

    public static ValueConverter<AnnotationId, Guid> AnnotationIdConverter { get; } = new(
        id => id.Value,
        value => AnnotationId.From(value));

    public static ValueConverter<InsightId, Guid> InsightIdConverter { get; } = new(
        id => id.Value,
        value => InsightId.From(value));

    public static ValueConverter<SearchQueryId, Guid> SearchQueryIdConverter { get; } = new(
        id => id.Value,
        value => SearchQueryId.From(value));

    public static ValueConverter<RecommendationId, Guid> RecommendationIdConverter { get; } = new(
        id => id.Value,
        value => RecommendationId.From(value));

    public static ValueConverter<Uri?, string?> UriConverter { get; } = new(
        uri => uri == null ? null : uri.ToString(),
        value => string.IsNullOrWhiteSpace(value) ? null : new Uri(value));

    public static ValueConverter<Doi?, string?> DoiConverter { get; } = new(
        doi => doi == null ? null : doi.Value,
        value => string.IsNullOrWhiteSpace(value) ? null : Doi.Create(value));
}
