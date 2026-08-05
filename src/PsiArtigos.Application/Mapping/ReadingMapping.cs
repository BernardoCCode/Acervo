using PsiArtigos.Application.DTOs.Reading;
using PsiArtigos.Domain.Aggregates;

namespace PsiArtigos.Application.Mapping;

public static class ReadingMapping
{
    public static ReadingSessionDto ToDto(this ReadingSession session)
    {
        return new ReadingSessionDto(
            session.Id.Value,
            session.ArticleId.Value,
            session.Progress.Percent,
            session.Progress.PageNumber,
            session.IsCompleted,
            session.LastOpenedAtUtc,
            session.OpenCount,
            session.ActiveReadingSeconds,
            session.Highlights.Select(h => new HighlightDto(
                h.Id.Value,
                h.Range.StartOffset,
                h.Range.EndOffset,
                h.Range.PageNumber,
                h.QuotedText,
                h.Color,
                h.Annotations.Select(a => new AnnotationDto(
                    a.Id.Value,
                    a.Note,
                    a.CreatedAtUtc)).ToList())).ToList());
    }

    public static ReaderPreferencesDto ToDto(this UserReaderSettings settings)
    {
        return new ReaderPreferencesDto(
            settings.Preferences.DarkMode,
            settings.Preferences.FontSize,
            settings.Preferences.PreferredTranslationLanguage);
    }
}
