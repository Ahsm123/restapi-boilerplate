using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.UserDtos;

public class CreateUserDto
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }

    //Only admin can set this role when creating a user, defaults to User
    public UserRole? Role { get; set; }
}