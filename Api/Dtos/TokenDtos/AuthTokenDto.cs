namespace Api.Dtos.TokenDtos;

public class AuthTokensDto
{
    public TokenInfoDto Access { get; set; } = new();
    public TokenInfoDto Refresh { get; set; } = new();
}
