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
    //TODO: Add EmailService when implemented

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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
            Password = dto.Password, //TODO: Implement password hasher with BCrypt
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var tokens = await _tokenService.GenerateAuthTokensAsync(createdUser);

        //TODO: Send verification email
        // var verifyEmailToken = await _tokenService.GenerateVerifyEmailTokenAsync(createdUser);
        // await _emailService.SendVerificationEmailAsync(createdUser.Email, verifyEmailToken);

        return new AuthResponse
        {
            User = MapToUserDto(createdUser),
            Tokens = tokens
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.ToLowerInvariant());

        if (user == null || user.Password != dto.Password) //TODO: Implement password hasher with BCrypt
        {
            throw new InvalidCredentialsException();
        }

        //TODO: Enforce email verification
        //if (!user.IsEmailVerified)
        //{
        //    throw new EmailNotVerifiedException(user.Email);
        //}

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

    public async Task<AuthTokensDto> RefreshTokenAsync(string refreshToken)
    {
        return await _tokenService.RefreshTokensAsync(refreshToken);
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        //Always return success so we dont reveal if the email exists
        try
        {
            var resetToken = await _tokenService.GenerateResetPasswordTokenAsync(email);
            //TODO: Send email
            // await _emailService.SendPasswordResetEmailAsync(email, resetToken);
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

        user.Password = newPassword; //TODO: Implement password hasher with BCrypt
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
            return; // No need to send if already verified
        }

        var verifyEmailToken = await _tokenService.GenerateVerifyEmailTokenAsync(user);
        //TODO: Send verification email
        // await _emailService.SendVerificationEmailAsync(user.Email, verifyEmailToken);
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
