using Api.Dtos.UserDtos;

namespace Api.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto userDto, bool isAdminCreating = false);
    Task<PaginatedResultDto<UserDto>> GetAllUsersAsync(UserQueryDto dto);
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto);
    Task DeleteUserAsync(Guid id);
}
