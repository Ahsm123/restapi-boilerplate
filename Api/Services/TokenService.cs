using Api.Configuration;
using Api.Dtos.TokenDtos;
using Api.Exceptions;
using Api.Exceptions.TokenExceptions;
using Api.Interfaces;
using Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _signingKey;

    public TokenService(
        ITokenRepository tokenRepository,
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            throw new ArgumentException("JWT Secret Key is not configured.", nameof(jwtSettings));

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    }

    public async Task<AuthTokensDto> GenerateAuthTokensAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var accessTokenExpires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var accessToken = GenerateJwtToken(user.Id, accessTokenExpires, TokenType.Access);
        var refreshToken = GenerateSecureToken();

        //Save refresh token to database
        await _tokenRepository.CreateAsync(new Token
        {
            Id = Guid.NewGuid(),
            TokenString = refreshToken,
            UserId = user.Id,
            Type = TokenType.Refresh,
            Expires = refreshTokenExpires,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return new AuthTokensDto
        {
            Access = new TokenInfoDto
            {
                Token = accessToken,
                Expires = accessTokenExpires
            },
            Refresh = new TokenInfoDto
            {
                Token = refreshToken,
                Expires = refreshTokenExpires
            }
        };
    }

    public async Task<AuthTokensDto> RefreshTokensAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty");

        var tokenRecord = await _tokenRepository.GetByTokenStringAsync(refreshToken);

        if (tokenRecord == null || tokenRecord.Type != TokenType.Refresh)
            throw new TokenNotFoundException(refreshToken, TokenType.Refresh);

        if (tokenRecord.Expires < DateTime.UtcNow)
            throw new TokenExpiredException(TokenType.Refresh);

        if (tokenRecord.Blacklisted)
            throw new TokenBlacklistedException(TokenType.Refresh);

        var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);
        if (user == null)
            throw new UserNotFoundException(tokenRecord.UserId);

        //Blacklist the old refresh token
        tokenRecord.Blacklisted = true;
        tokenRecord.UpdatedAt = DateTime.UtcNow;
        await _tokenRepository.UpdateAsync(tokenRecord);

        //Generate new tokens
        return await GenerateAuthTokensAsync(user);
    }

    public async Task<string> GenerateResetPasswordTokenAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email can't be null or empty");

        var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
        if (user == null)
            throw new UserNotFoundException(Guid.Empty);

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ResetPasswordTokenExpirationMinutes);
        var token = GenerateJwtToken(user.Id, expires, TokenType.ResetPassword);

        await _tokenRepository.CreateAsync(new Token
        {
            Id = Guid.NewGuid(),
            TokenString = token,
            UserId = user.Id,
            Type = TokenType.ResetPassword,
            Expires = expires,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return token;
    }

    public async Task<string> GenerateVerifyEmailTokenAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.VerifyEmailTokenExpirationMinutes);
        var token = GenerateJwtToken(user.Id, expires, TokenType.VerifyEmail);

        await _tokenRepository.CreateAsync(new Token
        {
            Id = Guid.NewGuid(),
            TokenString = token,
            UserId = user.Id,
            Type = TokenType.VerifyEmail,
            Expires = expires,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        return token;
    }

    public async Task<Token> ValidateTokenAsync(string token, TokenType expectedType)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token can't be null or empty");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var typeClaim = principal.FindFirst("type")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new InvalidTokenException("Invalid user ID in token");

            if (!Enum.TryParse<TokenType>(typeClaim, out var tokenType) || tokenType != expectedType)
                throw new InvalidTokenException($"Expected token type {expectedType}, but got {typeClaim}");

            // For tokens in db, verify they exist and aren't blacklisted
            if (expectedType != TokenType.Access)
            {
                var tokenRecord = await _tokenRepository.GetValidTokenAsync(token, expectedType, userId);
                if (tokenRecord == null)
                    throw new TokenNotFoundException(token, expectedType);

                if (tokenRecord.Blacklisted)
                    throw new TokenBlacklistedException(expectedType);

                return tokenRecord;
            }

            // For access tokens, create a temporary token object
            return new Token
            {
                TokenString = token,
                UserId = userId,
                Type = TokenType.Access,
                Expires = validatedToken.ValidTo
            };
        }
        catch (SecurityTokenException ex)
        {
            throw new InvalidTokenException(ex.Message);
        }
    }

    public async Task RevokeTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty");

        await _tokenRepository.BlacklistTokenAsync(token);
    }

    public async Task RevokeUserTokensAsync(Guid userId, TokenType? type = null)
    {
        await _tokenRepository.BlacklistUserTokensAsync(userId, type);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        await _tokenRepository.DeleteExpiredTokensAsync();
    }

    private string GenerateJwtToken(Guid userId, DateTime expires, TokenType type)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("type", type.ToString())
            }),
            Expires = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
