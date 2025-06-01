namespace Api.Dtos.TokenDtos;

public class TokenInfoDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}
