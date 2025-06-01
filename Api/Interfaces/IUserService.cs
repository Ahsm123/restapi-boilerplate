using Api.Dtos;

namespace Api.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto userDto);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto);
    Task DeleteUserAsync(Guid id);
}
