using PsiArtigos.Application.Interfaces;

namespace PsiArtigos.Infrastructure.External.AI;

/// <summary>
/// Deterministic local fallback used when no AI API key is configured.
/// </summary>
public sealed class LocalAiInsightService : IAiInsightPort
{
    public Task<string> SummarizeAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default)
    {
        var summary = string.IsNullOrWhiteSpace(articleAbstract)
            ? $"Resumo local de '{title}': este artigo discute o tema indicado no título. Configure a chave de IA para obter um resumo mais rico."
            : $"Resumo local de '{title}': {Trim(articleAbstract, 500)}";

        return Task.FromResult(summary);
    }

    public Task<string> ExplainForBeginnersAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default)
    {
        var explanation =
            $"Explicação para iniciantes sobre '{title}'. " +
            "Em termos simples: " +
            (string.IsNullOrWhiteSpace(articleAbstract)
                ? "o artigo apresenta ideias e evidências sobre o assunto. Configure a chave de IA para uma explicação mais detalhada."
                : Trim(articleAbstract, 450));

        return Task.FromResult(explanation);
    }

    public Task<string> TranslateAsync(
        string title,
        string? articleAbstract,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var body = string.IsNullOrWhiteSpace(articleAbstract) ? title : articleAbstract;
        var translation =
            $"[Tradução local {sourceLanguage} → {targetLanguage}] {Trim(body, 700)} " +
            "(Configure a chave de IA para tradução real.)";

        return Task.FromResult(translation);
    }

    private static string Trim(string value, int max)
        => value.Length <= max ? value.Trim() : value[..max].Trim() + "...";
}
