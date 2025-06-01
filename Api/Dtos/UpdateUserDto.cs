namespace Api.Dtos;

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Password { get; set; }
    public bool? IsEmailVerified { get; set; }
}
