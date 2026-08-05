namespace PsiArtigos.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "PsiArtigos";
    public string Audience { get; set; } = "PsiArtigos.Web";
    public string Key { get; set; } = "development-only-change-this-key-psiartigos-2026";
    public int ExpirationHours { get; set; } = 12;
    public int RememberMeDays { get; set; } = 30;
}
