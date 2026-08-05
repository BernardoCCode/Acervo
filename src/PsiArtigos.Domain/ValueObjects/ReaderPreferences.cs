using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class ReaderPreferences : ValueObject
{
    public const int MinFontSize = 12;
    public const int MaxFontSize = 28;

    public bool DarkMode { get; private set; }
    public int FontSize { get; private set; }
    public string? PreferredTranslationLanguage { get; private set; }

    private ReaderPreferences()
    {
    }

    private ReaderPreferences(bool darkMode, int fontSize, string? preferredTranslationLanguage)
    {
        DarkMode = darkMode;
        FontSize = fontSize;
        PreferredTranslationLanguage = preferredTranslationLanguage;
    }

    public static ReaderPreferences Default()
        => new(darkMode: true, fontSize: 16, preferredTranslationLanguage: null);

    public static ReaderPreferences Create(
        bool darkMode,
        int fontSize,
        string? preferredTranslationLanguage = null)
    {
        if (fontSize is < MinFontSize or > MaxFontSize)
            throw new DomainException($"Font size must be between {MinFontSize} and {MaxFontSize}.");

        return new ReaderPreferences(
            darkMode,
            fontSize,
            string.IsNullOrWhiteSpace(preferredTranslationLanguage)
                ? null
                : preferredTranslationLanguage.Trim().ToLowerInvariant());
    }

    public ReaderPreferences WithDarkMode(bool darkMode)
        => Create(darkMode, FontSize, PreferredTranslationLanguage);

    public ReaderPreferences WithFontSize(int fontSize)
        => Create(DarkMode, fontSize, PreferredTranslationLanguage);

    public ReaderPreferences WithPreferredTranslationLanguage(string? language)
        => Create(DarkMode, FontSize, language);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DarkMode;
        yield return FontSize;
        yield return PreferredTranslationLanguage;
    }
}
