using Api.Dtos.TokenDtos;
using Api.Dtos.UserDtos;

namespace Api.Dtos;

public class AuthResponse
{
    public UserDto User { get; set; } = new();
    public AuthTokensDto Tokens { get; set; } = new();
}
