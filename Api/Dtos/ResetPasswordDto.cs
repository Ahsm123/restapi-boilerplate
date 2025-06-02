using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class ResetPasswordDto
{
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
