using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AI;

/// <summary>
/// Uses an OpenAI-compatible model to design a topic-specific learning curriculum.
/// The configured Groq endpoint implements the same chat-completions contract.
/// </summary>
public sealed class OpenAiLearningService : IAiLearningPort
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public OpenAiLearningService(HttpClient httpClient, IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<LearningTrailPlan> PlanTrailAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest
        {
            Model = _options.Model,
            Temperature = 0.35,
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Messages =
            [
                new ChatMessage
                {
                    Role = "system",
                    Content = """
                        Você é um pesquisador e designer de currículos acadêmicos. Crie trilhas
                        progressivas, específicas ao pedido, para aprendizado por artigos científicos.

                        Regras:
                        - Decida entre 5 e 10 etapas conforme a complexidade real do tema.
                        - Não use a sequência genérica "introdução, fundamentos, clássico, recente".
                        - Cada etapa deve ensinar um conceito ou competência concreta e diferente.
                        - Decomponha pré-requisitos antes de tópicos que dependem deles.
                        - Títulos e justificativas devem citar conceitos específicos do tema.
                        - A justificativa deve explicar o que o leitor aprenderá, por que vem nesta
                          posição e o que deve observar no artigo.
                        - searchQuery deve ser uma consulta acadêmica precisa, preferencialmente em
                          inglês, com os principais termos técnicos. Não inclua anos variáveis.
                        - Use Beginner, Intermediate, Advanced, Classic ou RecentResearch em difficulty.
                        - Responda somente JSON válido no formato:
                          {
                            "topic": "nome claro e específico",
                            "steps": [
                              {
                                "title": "competência/conceito específico",
                                "difficulty": "Beginner",
                                "searchQuery": "precise scholarly query",
                                "rationale": "explicação específica em português"
                              }
                            ]
                          }
                        """
                },
                new ChatMessage
                {
                    Role = "user",
                    Content = $"Pedido do estudante: {prompt.Trim()}"
                }
            ]
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Não foi possível planejar a trilha com IA ({(int)response.StatusCode}): "
                + Truncate(error, 240));
        }

        var chat = await response.Content.ReadFromJsonAsync<ChatResponse>(
            cancellationToken: cancellationToken);
        var json = chat?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("A IA não retornou um plano de trilha.");

        var generated = JsonSerializer.Deserialize<GeneratedPlan>(
            StripCodeFence(json),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (generated is null
            || string.IsNullOrWhiteSpace(generated.Topic)
            || generated.Steps is null)
        {
            throw new InvalidOperationException("A IA retornou um plano de trilha inválido.");
        }

        var steps = generated.Steps
            .Where(IsValid)
            .Take(10)
            .Select(s => new LearningTrailStepPlan(
                s.Title!.Trim(),
                ParseDifficulty(s.Difficulty),
                s.SearchQuery!.Trim(),
                s.Rationale!.Trim()))
            .ToList();

        if (steps.Count < 3)
            throw new InvalidOperationException("A IA retornou poucas etapas utilizáveis.");

        return new LearningTrailPlan(generated.Topic.Trim(), steps);
    }

    private static bool IsValid(GeneratedStep step)
        => !string.IsNullOrWhiteSpace(step.Title)
            && !string.IsNullOrWhiteSpace(step.SearchQuery)
            && !string.IsNullOrWhiteSpace(step.Rationale);

    private static DifficultyLevel ParseDifficulty(string? value)
        => Enum.TryParse<DifficultyLevel>(value, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed)
                ? parsed
                : DifficultyLevel.Intermediate;

    private static string StripCodeFence(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            return trimmed;

        var firstNewline = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstNewline >= 0 && lastFence > firstNewline
            ? trimmed[(firstNewline + 1)..lastFence].Trim()
            : trimmed;
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("response_format")]
        public ResponseFormat? ResponseFormat { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }

    private sealed class ResponseFormat
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "json_object";
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }

    private sealed class GeneratedPlan
    {
        public string? Topic { get; set; }
        public List<GeneratedStep>? Steps { get; set; }
    }

    private sealed class GeneratedStep
    {
        public string? Title { get; set; }
        public string? Difficulty { get; set; }
        public string? SearchQuery { get; set; }
        public string? Rationale { get; set; }
    }
}
