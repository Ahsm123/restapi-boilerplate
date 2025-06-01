using Api.Models;

namespace Api.Interfaces;

public interface ITokenRepository
{
    Task<Token> CreateAsync(Token token);
    Task<Token?> GetValidTokenAsync(string tokenString, TokenType type, Guid userId);
    Task<Token?> GetByTokenStringAsync(string tokenString);
    Task<Token> UpdateAsync(Token token);
    Task DeleteAsync(Token token);
    Task<IEnumerable<Token>> GetUserTokensAsync(Guid userId, TokenType? type = null);
    Task BlacklistTokenAsync(string tokenString);
    Task BlacklistUserTokensAsync(Guid userId, TokenType? type = null);
    Task DeleteExpiredTokensAsync();
}
