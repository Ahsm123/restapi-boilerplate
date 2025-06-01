using Api.Models;

namespace Api.Exceptions.TokenExceptions;

public class TokenNotFoundException : DomainException
{
    public TokenNotFoundException(string token, TokenType type)
        : base($"Token of type '{type}' not found or invalid")
    {
    }
}
