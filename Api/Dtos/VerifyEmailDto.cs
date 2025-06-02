using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class VerifyEmailDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
