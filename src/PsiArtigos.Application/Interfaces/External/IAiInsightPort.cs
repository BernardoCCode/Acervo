namespace PsiArtigos.Application.Interfaces;

public interface IAiInsightPort
{
    Task<string> SummarizeAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default);

    Task<string> ExplainForBeginnersAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default);

    Task<string> TranslateAsync(
        string title,
        string? articleAbstract,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}
