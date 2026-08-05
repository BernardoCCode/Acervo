namespace PsiArtigos.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException For<T>(Guid id)
        => new($"{typeof(T).Name} '{id}' was not found.");
}
