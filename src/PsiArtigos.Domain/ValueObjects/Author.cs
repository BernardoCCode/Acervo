using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class Author : ValueObject
{
    public string Name { get; private set; } = null!;
    public string? Orcid { get; private set; }
    public string? Affiliation { get; private set; }

    private Author()
    {
    }

    private Author(string name, string? orcid, string? affiliation)
    {
        Name = name;
        Orcid = orcid;
        Affiliation = affiliation;
    }

    public static Author Create(string name, string? orcid = null, string? affiliation = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Author name is required.");

        return new Author(
            name.Trim(),
            string.IsNullOrWhiteSpace(orcid) ? null : orcid.Trim(),
            string.IsNullOrWhiteSpace(affiliation) ? null : affiliation.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Orcid;
        yield return Affiliation;
    }
}
