using System.Text.RegularExpressions;
using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed partial class Doi : ValueObject
{
    public string Value { get; private set; } = null!;

    private Doi()
    {
    }

    private Doi(string value) => Value = value;

    public static Doi Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("DOI is required.");

        var normalized = value.Trim()
            .Replace("https://doi.org/", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://doi.org/", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (!DoiRegex().IsMatch(normalized))
            throw new DomainException("DOI format is invalid.");

        return new Doi(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^10\.\d{4,9}/\S+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DoiRegex();
}
