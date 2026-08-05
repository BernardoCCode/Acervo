namespace PsiArtigos.Infrastructure.Options;

public sealed class AcademicSearchOptions
{
    public const string SectionName = "AcademicSearch";

    public bool EnableOpenAlex { get; set; } = true;
    public bool EnableArxiv { get; set; } = true;
    public bool EnableCrossref { get; set; } = false;
    public bool EnableEuropePmc { get; set; } = true;
    public bool EnableSemanticScholar { get; set; } = true;
    public int MaxResultsPerSource { get; set; } = 40;
    public string? OpenAlexMailto { get; set; }
}
