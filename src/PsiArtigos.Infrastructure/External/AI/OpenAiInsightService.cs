using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using PsiArtigos.Application.Interfaces;
using PsiArtigos.Infrastructure.Options;

namespace PsiArtigos.Infrastructure.External.AI;

/// <summary>
/// OpenAI-compatible chat-completions client for summaries, beginner explanations
/// and translations. Used whenever an API key is configured.
/// </summary>
public sealed class OpenAiInsightService : IAiInsightPort
{
    private readonly HttpClient _httpClient;
    private readonly AiOptions _options;

    public OpenAiInsightService(HttpClient httpClient, IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public Task<string> SummarizeAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default)
    {
        var text = BuildInput(title, articleAbstract);
        const string system =
            "Você é um assistente acadêmico que resume textos científicos em português "
            + "do Brasil de forma clara, fiel e objetiva. Nunca invente informações que não "
            + "estejam no texto. Não use marcadores nem títulos; escreva em prosa corrida.";
        var user =
            "Resuma o texto a seguir em português do Brasil, em 3 a 6 frases. "
            + "Quando o texto permitir, destaque o objetivo, o método e os principais achados. "
            + "Se for apenas um trecho curto, resuma somente esse trecho.\n\n"
            + text;

        return CompleteAsync(system, user, 0.3, cancellationToken);
    }

    public Task<string> ExplainForBeginnersAsync(
        string title,
        string? articleAbstract,
        CancellationToken cancellationToken = default)
    {
        var text = BuildInput(title, articleAbstract);
        const string system =
            "Você é um professor que explica conteúdos acadêmicos para leigos, em português "
            + "do Brasil. Use linguagem simples e acolhedora, frases curtas e analogias quando "
            + "ajudar. Sempre que usar um termo técnico, explique-o em seguida com poucas palavras. "
            + "Baseie-se apenas no texto fornecido.";
        var user =
            "Explique o texto a seguir para alguém que está começando a estudar o assunto, "
            + "em português do Brasil, em 1 a 3 parágrafos curtos. Comece dizendo, em uma frase, "
            + "sobre o que é o texto.\n\n"
            + text;

        return CompleteAsync(system, user, 0.4, cancellationToken);
    }

    public async Task<string> TranslateAsync(
        string title,
        string? articleAbstract,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var text = string.IsNullOrWhiteSpace(articleAbstract) ? title : articleAbstract!;
        var targetName = LanguageName(targetLanguage);
        const string system =
            "Você é um tradutor profissional. Traduza com naturalidade e fidelidade, "
            + "preservando o sentido e o tom. Responda apenas com a tradução, sem comentários, "
            + "aspas ou notas.";
        var chunks = SplitTranslationChunks(text, 4_500);
        var translated = new List<string>(chunks.Count);

        for (var index = 0; index < chunks.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunk = chunks[index];
            var user =
                $"Detecte automaticamente o idioma de origem e traduza o trecho acadêmico "
                + $"a seguir para {targetName}. Preserve parágrafos, títulos, listas, referências "
                + "e termos técnicos. Não resuma e não omita conteúdo. Se já estiver no idioma "
                + "de destino, apenas devolva-o revisado.\n\n"
                + chunk;

            translated.Add(await CompleteAsync(system, user, 0.15, cancellationToken));

            // Groq's free tier has a tokens-per-minute limit. A small pause keeps
            // long articles progressing instead of failing halfway through.
            if (index < chunks.Count - 1)
                await Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
        }

        return string.Join("\n\n", translated);
    }

    private static List<string> SplitTranslationChunks(string text, int maxLength)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
        var paragraphs = normalized
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var chunks = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > maxLength)
            {
                FlushCurrent();
                var remaining = paragraph;
                while (remaining.Length > maxLength)
                {
                    var splitAt = remaining.LastIndexOfAny(
                        [' ', '\n', '.', ';', ','],
                        maxLength - 1,
                        Math.Min(700, maxLength));
                    if (splitAt < maxLength - 700)
                        splitAt = maxLength;

                    chunks.Add(remaining[..splitAt].Trim());
                    remaining = remaining[splitAt..].TrimStart();
                }
                if (remaining.Length > 0)
                    chunks.Add(remaining);
                continue;
            }

            var extra = current.Length == 0 ? paragraph.Length : paragraph.Length + 2;
            if (current.Length + extra > maxLength)
                FlushCurrent();

            if (current.Length > 0)
                current.Append("\n\n");
            current.Append(paragraph);
        }

        FlushCurrent();
        return chunks.Count > 0 ? chunks : [normalized];

        void FlushCurrent()
        {
            if (current.Length == 0)
                return;
            chunks.Add(current.ToString());
            current.Clear();
        }
    }

    private static string BuildInput(string title, string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return $"Título: {title}";
        return $"Título: {title}\n\nTexto:\n{body.Trim()}";
    }

    private static string LanguageName(string code) => code.Trim().ToLowerInvariant() switch
    {
        "pt" or "pt-br" or "por" => "português do Brasil",
        "en" or "en-us" or "eng" => "inglês (English)",
        _ => code,
    };

    private async Task<string> CompleteAsync(
        string system,
        string user,
        double temperature,
        CancellationToken cancellationToken)
    {
        var payload = new ChatRequest
        {
            Model = _options.Model,
            Temperature = temperature,
            Messages =
            [
                new ChatMessage { Role = "system", Content = system },
                new ChatMessage { Role = "user", Content = user },
            ],
        };

        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(payload),
            };
            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ChatResponse>(
                    cancellationToken: cancellationToken);

                var content = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("A IA não retornou conteúdo.");

                return content;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if ((int)response.StatusCode == 429 && attempt < 2)
            {
                var delay = GetRetryDelay(response, body, attempt);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            throw new InvalidOperationException(
                DescribeOpenAiFailure(response.StatusCode, body));
        }

        throw new InvalidOperationException("A IA excedeu o limite temporário de uso.");
    }

    private static TimeSpan GetRetryDelay(
        HttpResponseMessage response,
        string body,
        int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is { } headerDelay)
            return TimeSpan.FromSeconds(Math.Clamp(headerDelay.TotalSeconds + 1, 2, 90));

        var minutes = Regex.Match(
            body,
            @"try again in\s+(?<m>\d+)m(?<s>[\d.]+)s",
            RegexOptions.IgnoreCase);
        if (minutes.Success
            && double.TryParse(
                minutes.Groups["m"].Value,
                System.Globalization.CultureInfo.InvariantCulture,
                out var m)
            && double.TryParse(
                minutes.Groups["s"].Value,
                System.Globalization.CultureInfo.InvariantCulture,
                out var s))
        {
            return TimeSpan.FromSeconds(Math.Clamp(m * 60 + s + 1, 2, 90));
        }

        var seconds = Regex.Match(
            body,
            @"try again in\s+(?<s>[\d.]+)s",
            RegexOptions.IgnoreCase);
        if (seconds.Success
            && double.TryParse(
                seconds.Groups["s"].Value,
                System.Globalization.CultureInfo.InvariantCulture,
                out var secondsValue))
        {
            return TimeSpan.FromSeconds(Math.Clamp(secondsValue + 1, 2, 90));
        }

        return TimeSpan.FromSeconds(15 * (attempt + 1));
    }

    private static string DescribeOpenAiFailure(
        System.Net.HttpStatusCode status,
        string body)
    {
        var lower = body.ToLowerInvariant();
        if (lower.Contains("insufficient_quota")
            || lower.Contains("credit_balance_exhausted"))
        {
            return "Sua conta OpenAI está sem créditos. "
                + "Adicione créditos em platform.openai.com → Billing e tente de novo.";
        }

        if ((int)status == 429)
        {
            return "A IA atingiu o limite temporário de tradução. "
                + "Aguarde um minuto e tente novamente.";
        }

        if (status is System.Net.HttpStatusCode.Unauthorized
            or System.Net.HttpStatusCode.Forbidden
            || lower.Contains("invalid_api_key"))
        {
            return "A chave da OpenAI é inválida ou não tem permissão. "
                + "Confira com: dotnet user-secrets set \"AI:ApiKey\" \"sk-...\" "
                + "--project src/PsiArtigos.Api";
        }

        return $"A IA respondeu com erro ({(int)status}). {Truncate(body, 220)}";
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-mini";

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

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
}
