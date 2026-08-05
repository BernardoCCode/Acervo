using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class SearchFilters : ValueObject
{
    private readonly List<SourceSystem> _sources = [];

    public int? YearMin { get; private set; }
    public int? YearMax { get; private set; }
    public string? Language { get; private set; }
    public StudyType? StudyType { get; private set; }
    public int? MinCitations { get; private set; }
    public IReadOnlyCollection<SourceSystem> Sources => _sources.AsReadOnly();

    private SearchFilters()
    {
    }

    private SearchFilters(
        int? yearMin,
        int? yearMax,
        string? language,
        StudyType? studyType,
        int? minCitations,
        IEnumerable<SourceSystem> sources)
    {
        YearMin = yearMin;
        YearMax = yearMax;
        Language = language;
        StudyType = studyType;
        MinCitations = minCitations;
        _sources.AddRange(sources.Distinct());
    }

    public static SearchFilters Empty() => new(null, null, null, null, null, []);

    public static SearchFilters Create(
        int? yearMin = null,
        int? yearMax = null,
        string? language = null,
        StudyType? studyType = null,
        int? minCitations = null,
        IEnumerable<SourceSystem>? sources = null)
    {
        if (yearMin is not null && yearMax is not null && yearMin > yearMax)
            throw new DomainException("Search year minimum cannot be greater than year maximum.");

        if (yearMin is < 1400)
            throw new DomainException("Search year minimum is out of range.");

        if (yearMax is not null && yearMax > DateTime.UtcNow.Year + 1)
            throw new DomainException("Search year maximum is out of range.");

        if (minCitations is < 0)
            throw new DomainException("Minimum citations cannot be negative.");

        if (studyType is not null && !Enum.IsDefined(studyType.Value))
            throw new DomainException("Invalid study type filter.");

        var sourceList = (sources ?? []).ToList();
        if (sourceList.Any(s => !Enum.IsDefined(s)))
            throw new DomainException("Invalid source system filter.");

        return new SearchFilters(
            yearMin,
            yearMax,
            string.IsNullOrWhiteSpace(language) ? null : language.Trim().ToLowerInvariant(),
            studyType,
            minCitations,
            sourceList);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return YearMin;
        yield return YearMax;
        yield return Language;
        yield return StudyType;
        yield return MinCitations;

        foreach (var source in _sources.OrderBy(s => s))
            yield return source;
    }
}
