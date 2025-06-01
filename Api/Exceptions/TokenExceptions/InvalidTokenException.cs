namespace Api.Exceptions.TokenExceptions;

public class InvalidTokenException : DomainException
{
    public InvalidTokenException(string message)
        : base($"Invalid token: {message}")
    {
    }
}