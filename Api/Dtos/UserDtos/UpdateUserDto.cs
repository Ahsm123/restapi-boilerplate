using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.UserDtos;

public class UpdateUserDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [MinLength(8)]
    public string? Password { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public bool? IsEmailVerified { get; set; }
}
