namespace PsiArtigos.Infrastructure.Options;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>
    /// When empty, Infrastructure uses a deterministic local fallback (useful for development).
    /// </summary>
    public string? ApiKey { get; set; }

    public string Model { get; set; } = "gpt-4o-mini";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
}
