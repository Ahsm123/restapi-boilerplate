using Api.Interfaces;
using Api.Models;

namespace Api.Data;

public class TokenRepository : ITokenRepository
{
    public Task BlacklistTokenAsync(string tokenString)
    {
        throw new NotImplementedException();
    }

    public Task BlacklistUserTokensAsync(Guid userId, TokenType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<Token> CreateAsync(Token token)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Token token)
    {
        throw new NotImplementedException();
    }

    public Task DeleteExpiredTokensAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Token?> GetByTokenStringAsync(string tokenString)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Token>> GetUserTokensAsync(Guid userId, TokenType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<Token?> GetValidTokenAsync(string tokenString, TokenType type, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Token> UpdateAsync(Token token)
    {
        throw new NotImplementedException();
    }
}
