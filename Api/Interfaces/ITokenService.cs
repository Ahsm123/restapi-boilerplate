using Api.Dtos.TokenDtos;
using Api.Models;

namespace Api.Interfaces;

public interface ITokenService
{
    Task<AuthTokensDto> GenerateAuthTokensAsync(User user);
    Task<AuthTokensDto> RefreshTokensAsync(string refreshToken);
    Task<string> GenerateResetPasswordTokenAsync(string email);
    Task<string> GenerateVerifyEmailTokenAsync(User user);
    Task<Token> ValidateTokenAsync(string token, TokenType expectedType);
    Task RevokeTokenAsync(string token);
    Task RevokeUserTokensAsync(Guid userId, TokenType? type = null);
    Task CleanupExpiredTokensAsync();
}
