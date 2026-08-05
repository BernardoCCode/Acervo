using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class PublicationInfo : ValueObject
{
    public string? Venue { get; private set; }
    public int? Year { get; private set; }
    public Doi? Doi { get; private set; }
    public Uri? Url { get; private set; }

    private PublicationInfo()
    {
    }

    private PublicationInfo(string? venue, int? year, Doi? doi, Uri? url)
    {
        Venue = venue;
        Year = year;
        Doi = doi;
        Url = url;
    }

    public static PublicationInfo Create(
        string? venue = null,
        int? year = null,
        string? doi = null,
        string? url = null)
    {
        if (year is not null && (year < 1400 || year > DateTime.UtcNow.Year + 1))
            throw new DomainException("Publication year is out of range.");

        Uri? parsedUrl = null;
        if (!string.IsNullOrWhiteSpace(url))
        {
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out parsedUrl)
                || (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
            {
                throw new DomainException("Publication url must be an absolute http/https url.");
            }
        }

        return new PublicationInfo(
            string.IsNullOrWhiteSpace(venue) ? null : venue.Trim(),
            year,
            string.IsNullOrWhiteSpace(doi) ? null : Doi.Create(doi),
            parsedUrl);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Venue;
        yield return Year;
        yield return Doi;
        yield return Url?.ToString();
    }
}
