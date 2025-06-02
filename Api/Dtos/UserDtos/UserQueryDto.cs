using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.Dtos.UserDtos;

public class UserQueryDto
{
    public string? Name { get; set; }
    public UserRole? Role { get; set; }
    public string? SortBy { get; set; } = "name:asc";

    [Range(1, 100)]
    public int Limit { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
}
