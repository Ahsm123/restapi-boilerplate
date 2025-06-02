using Api.Dtos;
using Api.Dtos.TokenDtos;
using Api.Dtos.UserDtos;

namespace Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(CreateUserDto request);
    Task<AuthResponse> LoginAsync(LoginDto request);
    Task LogoutAsync(string refreshToken);
    Task<AuthTokensDto> RefreshTokensAsync(string refreshToken);
    Task SendPasswordResetEmailAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task SendVerificationEmailAsync(Guid userId);
    Task VerifyEmailAsync(string token);
}
