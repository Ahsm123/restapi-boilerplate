using Api.Models;

namespace Api.Dtos.UserDtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; }
}