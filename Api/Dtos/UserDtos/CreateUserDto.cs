using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.UserDtos;

public class CreateUserDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}