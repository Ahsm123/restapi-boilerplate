using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public class CreateTokenDto
{
    [Required]
    public string TokenString { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public TokenType Type { get; set; }

    [Required]
    public DateTime Expires { get; set; }
}
