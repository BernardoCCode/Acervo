using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;
using PsiArtigos.Domain.ValueObjects;

namespace PsiArtigos.Domain.Entities;

public sealed class CitationLink : Entity<CitationLinkId>
{
    public ArticleId FromArticleId { get; private set; }
    public ArticleId ToArticleId { get; private set; }
    public CitationLinkType LinkType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private CitationLink()
    {
    }

    public static CitationLink Create(
        ArticleId fromArticleId,
        ArticleId toArticleId,
        CitationLinkType linkType,
        DateTime? createdAtUtc = null)
    {
        if (fromArticleId == toArticleId)
            throw new DomainException("An article cannot cite itself.");

        if (!Enum.IsDefined(linkType))
            throw new DomainException("Invalid citation link type.");

        return new CitationLink
        {
            Id = CitationLinkId.New(),
            FromArticleId = fromArticleId,
            ToArticleId = toArticleId,
            LinkType = linkType,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow
        };
    }
}
