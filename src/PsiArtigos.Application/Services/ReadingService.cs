using PsiArtigos.Application.Common.Exceptions;
using PsiArtigos.Application.DTOs.Reading;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Application.Mapping;
using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Entities;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Application.Services;

public sealed class ReadingService
{
    private readonly IReadingSessionRepository _sessions;
    private readonly IUserReaderSettingsRepository _settings;
    private readonly IArticleRepository _articles;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ReadingService(
        IReadingSessionRepository sessions,
        IUserReaderSettingsRepository settings,
        IArticleRepository articles,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _settings = settings;
        _articles = articles;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReadingSessionDto> OpenSessionAsync(
        Guid articleId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var typedArticleId = ArticleId.From(articleId);

        var article = await _articles.GetByIdAsync(typedArticleId, cancellationToken);
        if (article is null)
            throw NotFoundException.For<Article>(articleId);

        var session = await _sessions.GetOrCreateAsync(
            userId,
            typedArticleId,
            cancellationToken);

        session.Touch();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<ReadingSessionDto> UpdateProgressAsync(
        UpdateReadingProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var session = await _sessions.GetByIdAsync(
            ReadingSessionId.From(request.SessionId),
            cancellationToken);

        if (session is null)
            throw NotFoundException.For<ReadingSession>(request.SessionId);

        session.EnsureOwnedBy(userId);
        session.UpdateProgress(
            ReadingProgress.Create(
                request.Percent,
                request.PageNumber,
                request.CharacterOffset));
        session.RecordActiveReading(request.ActiveSeconds);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<ReadingSessionDto> AddHighlightAsync(
        AddHighlightRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var session = await _sessions.GetByIdAsync(
            ReadingSessionId.From(request.SessionId),
            cancellationToken);

        if (session is null)
            throw NotFoundException.For<ReadingSession>(request.SessionId);

        session.EnsureOwnedBy(userId);

        // Do not stack overlapping highlights (causes duplicated words in the reader).
        var overlapsExisting = session.Highlights.Any(h =>
            h.Range.StartOffset < request.EndOffset
            && h.Range.EndOffset > request.StartOffset);

        if (overlapsExisting)
            return session.ToDto();

        var highlight = session.AddHighlight(
            TextRange.Create(request.StartOffset, request.EndOffset, request.PageNumber),
            request.QuotedText,
            request.Color);

        if (!string.IsNullOrWhiteSpace(request.Note))
            session.AddAnnotation(highlight.Id, request.Note);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<ReadingSessionDto> RemoveHighlightAsync(
        Guid sessionId,
        Guid highlightId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();

        var session = await _sessions.GetByIdAsync(
            ReadingSessionId.From(sessionId),
            cancellationToken);

        if (session is null)
            throw NotFoundException.For<ReadingSession>(sessionId);

        session.EnsureOwnedBy(userId);

        if (!session.RemoveHighlight(HighlightId.From(highlightId)))
            throw NotFoundException.For<Highlight>(highlightId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToDto();
    }

    public async Task<ReaderPreferencesDto> GetPreferencesAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var settings = await _settings.GetByUserIdAsync(userId, cancellationToken);

        if (settings is not null)
            return settings.ToDto();

        settings = UserReaderSettings.Create(userId);
        await _settings.AddAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return settings.ToDto();
    }

    public async Task<ReaderPreferencesDto> UpdatePreferencesAsync(
        UpdateReaderPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.GetRequiredUserId();
        var preferences = ReaderPreferences.Create(
            request.DarkMode,
            request.FontSize,
            request.PreferredTranslationLanguage);

        var settings = await _settings.GetByUserIdAsync(userId, cancellationToken);

        if (settings is null)
        {
            settings = UserReaderSettings.Create(userId, preferences);
            await _settings.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.UpdatePreferences(preferences);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return settings.ToDto();
    }
}
