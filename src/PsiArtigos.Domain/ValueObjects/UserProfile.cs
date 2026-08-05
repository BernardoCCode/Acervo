using PsiArtigos.Domain.Abstractions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class UserProfile : ValueObject
{
    private readonly List<TopicTag> _interests = [];

    public string? DisplayName { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public IReadOnlyCollection<TopicTag> Interests => _interests.AsReadOnly();

    private UserProfile()
    {
    }

    private UserProfile(string? displayName, string? preferredLanguage, IEnumerable<TopicTag> interests)
    {
        DisplayName = displayName;
        PreferredLanguage = preferredLanguage;
        _interests.AddRange(interests.Distinct());
    }

    public static UserProfile Create(
        string? displayName = null,
        string? preferredLanguage = null,
        IEnumerable<TopicTag>? interests = null)
    {
        return new UserProfile(
            string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            string.IsNullOrWhiteSpace(preferredLanguage) ? null : preferredLanguage.Trim().ToLowerInvariant(),
            interests ?? []);
    }

    public UserProfile WithDisplayName(string? displayName)
        => Create(displayName, PreferredLanguage, _interests);

    public UserProfile WithPreferredLanguage(string? preferredLanguage)
        => Create(DisplayName, preferredLanguage, _interests);

    public UserProfile WithInterests(IEnumerable<TopicTag> interests)
        => Create(DisplayName, PreferredLanguage, interests);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DisplayName;
        yield return PreferredLanguage;

        foreach (var interest in _interests.OrderBy(i => i.Value))
            yield return interest;
    }
}
