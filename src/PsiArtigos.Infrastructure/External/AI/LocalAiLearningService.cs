using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Infrastructure.External.AI;

/// <summary>
/// Deterministic local fallback that builds a sensible learning trail without an LLM.
/// </summary>
public sealed class LocalAiLearningService : IAiLearningPort
{
    public Task<LearningTrailPlan> PlanTrailAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var topic = ExtractTopic(prompt);

        var plan = new LearningTrailPlan(
            Topic: topic,
            Steps:
            [
                new LearningTrailStepPlan(
                    Title: $"Introdução a {topic}",
                    Difficulty: DifficultyLevel.Beginner,
                    SearchQuery: $"{topic} introduction tutorial",
                    Rationale: "Comece com um texto acessível para criar base conceitual."),
                new LearningTrailStepPlan(
                    Title: $"Fundamentos de {topic}",
                    Difficulty: DifficultyLevel.Intermediate,
                    SearchQuery: $"{topic} fundamentals review",
                    Rationale: "Aprofunde os conceitos centrais com uma revisão intermediária."),
                new LearningTrailStepPlan(
                    Title: $"Artigo clássico de {topic}",
                    Difficulty: DifficultyLevel.Classic,
                    SearchQuery: $"{topic} seminal classic paper",
                    Rationale: "Leia uma referência clássica da área."),
                new LearningTrailStepPlan(
                    Title: $"Pesquisa recente em {topic}",
                    Difficulty: DifficultyLevel.RecentResearch,
                    SearchQuery: $"{topic} recent advances 2024 2025",
                    Rationale: "Feche a trilha com pesquisas atuais.")
            ]);

        return Task.FromResult(plan);
    }

    private static string ExtractTopic(string prompt)
    {
        var cleaned = prompt.Trim();
        var prefixes = new[]
        {
            "quero aprender",
            "aprender",
            "estudar",
            "i want to learn",
            "learn"
        };

        foreach (var prefix in prefixes)
        {
            if (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[prefix.Length..].Trim(' ', '.', ',', '!', '?');
                break;
            }
        }

        cleaned = cleaned
            .Replace("do zero", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("from scratch", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim(' ', '.', ',', '!', '?');

        return string.IsNullOrWhiteSpace(cleaned) ? "Tópico geral" : cleaned;
    }
}
