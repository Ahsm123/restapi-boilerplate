using Api.Dtos;
using Api.Dtos.TokenDtos;
using Api.Dtos.UserDtos;
using Api.Exceptions;
using Api.Interfaces;
using Api.Models;
using Api.Validations;

namespace Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<AuthResponse> RegisterAsync(CreateUserDto dto)
    {
        if (!PasswordValidator.IsValid(dto.Password))
        {
            throw new WeakPasswordException();
        }

        if (await _userRepository.IsEmailTakenAsync(dto.Email))
        {
            throw new EmailAlreadyTakenException(dto.Email);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email.ToLowerInvariant(),
            Password = PasswordHasher.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var tokens = await _tokenService.GenerateAuthTokensAsync(createdUser);

        var verifyEmailToken = await _tokenService.GenerateVerifyEmailTokenAsync(createdUser);
        await _emailService.SendVerificationEmailAsync(createdUser.Email, verifyEmailToken);

        return new AuthResponse
        {
            User = MapToUserDto(createdUser),
            Tokens = tokens
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant());

        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.Password))
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsEmailVerified)
        {
            throw new EmailNotVerifiedException();
        }

        var tokens = await _tokenService.GenerateAuthTokensAsync(user);

        return new AuthResponse
        {
            User = MapToUserDto(user),
            Tokens = tokens
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeTokenAsync(refreshToken);
    }

    public async Task<AuthTokensDto> RefreshTokensAsync(string refreshToken)
    {
        return await _tokenService.RefreshTokensAsync(refreshToken);
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        //Always return success so we dont reveal if the email exists
        try
        {
            var resetToken = await _tokenService.GenerateResetPasswordTokenAsync(email);
            await _emailService.SendResetPasswordEmailAsync(email, resetToken);
        }
        catch (UserNotFoundException)
        {
            //Ignore the exception to prevent revealing if the email exists
        }
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        if (!PasswordValidator.IsValid(newPassword))
        {
            throw new WeakPasswordException();
        }

        var tokenRecord = await _tokenService.ValidateTokenAsync(token, TokenType.ResetPassword);
        var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);

        if (user == null)
        {
            throw new UserNotFoundException(tokenRecord.UserId);
        }

        user.Password = PasswordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _tokenService.RevokeTokenAsync(token);
    }

    public async Task SendVerificationEmailAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        if (user.IsEmailVerified)
        {
            return; //Already verified
        }

        var verificationToken = await _tokenService.GenerateVerifyEmailTokenAsync(user);
        await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

    }

    public async Task VerifyEmailAsync(string token)
    {
        var tokenRecord = await _tokenService.ValidateTokenAsync(token, TokenType.VerifyEmail);
        var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);

        if (user == null)
            throw new UserNotFoundException(tokenRecord.UserId);

        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _tokenService.RevokeTokenAsync(token);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified
        };

    }
}
