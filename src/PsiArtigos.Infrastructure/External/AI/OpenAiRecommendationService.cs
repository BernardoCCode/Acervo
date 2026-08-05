using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AI;

public sealed class OpenAiRecommendationService : IAiRecommendationPort
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public OpenAiRecommendationService(HttpClient httpClient, IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<RecommendationProfilePlan> AnalyzeAsync(
        RecommendationProfileInput input,
        CancellationToken cancellationToken = default)
    {
        var payload = new ChatRequest
        {
            Model = _options.Model,
            Temperature = 0.2,
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Messages =
            [
                new ChatMessage
                {
                    Role = "system",
                    Content = """
                        Você analisa histórico de leitura acadêmica para criar recomendações fortes.
                        Identifique de 3 a 6 interesses específicos, conexões conceituais adjacentes e
                        consultas acadêmicas em inglês. Dê prioridade às leituras concluídas e
                        favoritos; não repita apenas os títulos. Evite tópicos genéricos. Responda
                        somente JSON: {"topics":["..."],"searchQueries":["..."],"summary":"..."}.
                        """
                },
                new ChatMessage
                {
                    Role = "user",
                    Content = JsonSerializer.Serialize(input)
                }
            ]
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var chat = await response.Content.ReadFromJsonAsync<ChatResponse>(
            cancellationToken: cancellationToken);
        var raw = chat?.Choices.FirstOrDefault()?.Message.Content;
        var generated = JsonSerializer.Deserialize<GeneratedPlan>(
            raw ?? "{}",
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var topics = generated?.Topics?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList() ?? [];
        var queries = generated?.SearchQueries?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList() ?? [];
        if (queries.Count == 0)
            throw new InvalidOperationException("A IA não retornou consultas de recomendação.");
        return new RecommendationProfilePlan(
            topics,
            queries,
            generated?.Summary?.Trim() ?? "Perfil acadêmico personalizado.");
    }

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = null!;
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
        [JsonPropertyName("response_format")] public ResponseFormat ResponseFormat { get; set; } = null!;
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];
    }

    private sealed class ResponseFormat
    {
        [JsonPropertyName("type")] public string Type { get; set; } = null!;
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = null!;
        [JsonPropertyName("content")] public string Content { get; set; } = null!;
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")] public List<Choice> Choices { get; set; } = [];
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public ChatMessage Message { get; set; } = null!;
    }

    private sealed class GeneratedPlan
    {
        public List<string>? Topics { get; set; }
        public List<string>? SearchQueries { get; set; }
        public string? Summary { get; set; }
    }
}
