using PsiArtigos.Domain.Aggregates;
using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Domain.Services;

public interface ICitationFormatter
{
    string Format(Article article, CitationStyle style);
}
