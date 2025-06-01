using Api.Models;

namespace Api.Exceptions.TokenExceptions;

public class TokenExpiredException : DomainException
{
    public TokenExpiredException(TokenType type)
        : base($"Token of type '{type}' has expired")
    {
    }
}