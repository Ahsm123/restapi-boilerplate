using Api.Models;

namespace Api.Exceptions.TokenExceptions;

public class TokenBlacklistedException : DomainException
{
    public TokenBlacklistedException(TokenType type)
        : base($"Token of type '{type}' has been revoked")
    {
    }
}
