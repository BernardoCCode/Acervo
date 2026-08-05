namespace PsiArtigos.Application.Common.Exceptions;

public sealed class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message = "User is not authenticated.")
        : base(message)
    {
    }
}
