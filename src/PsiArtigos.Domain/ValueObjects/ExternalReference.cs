using PsiArtigos.Domain.Abstractions;
using PsiArtigos.Domain.Enums;
using PsiArtigos.Domain.Exceptions;

namespace PsiArtigos.Domain.ValueObjects;

public sealed class ExternalReference : ValueObject
{
    public SourceSystem System { get; private set; }
    public string ExternalId { get; private set; } = null!;

    private ExternalReference()
    {
    }

    private ExternalReference(SourceSystem system, string externalId)
    {
        System = system;
        ExternalId = externalId;
    }

    public static ExternalReference Create(SourceSystem system, string externalId)
    {
        if (!Enum.IsDefined(system))
            throw new DomainException("Invalid source system.");

        if (string.IsNullOrWhiteSpace(externalId))
            throw new DomainException("External id is required.");

        return new ExternalReference(system, externalId.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return System;
        yield return ExternalId;
    }
}
