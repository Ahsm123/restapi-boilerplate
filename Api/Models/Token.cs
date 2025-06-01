using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public enum TokenType
{
    Access,
    Refresh,
    ResetPassword,
    VerifyEmail
}
public class Token
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string TokenString { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; }

    [Required]
    public TokenType Type { get; set; }

    [Required]
    public DateTime Expires { get; set; }

    public bool BlackListed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
