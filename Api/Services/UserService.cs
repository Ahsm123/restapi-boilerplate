using Api.Dtos.UserDtos;
using Api.Exceptions;
using Api.Interfaces;
using Api.Models;
using Api.Validations;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto userDto, bool isAdminCreating = false)
    {
        //TODO: add admin created user.
        if (userDto == null)
        {
            throw new ArgumentNullException(nameof(userDto), "User data cannot be null.");
        }

        if (await _userRepository.IsEmailTakenAsync(userDto.Email))
        {
            throw new EmailAlreadyTakenException(userDto.Email);
        }

        //TODO: Add password validation
        //TODO: Hash password

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = userDto.Name,
            Email = userDto.Email.ToLowerInvariant(),
            Password = userDto.Password,
            Role = isAdminCreating && userDto.Role.HasValue
                    ? userDto.Role.Value
                    : UserRole.User, // Default role for regular registration
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var createdUser = await _userRepository.CreateAsync(user);
        return MapToDto(createdUser);
    }

    public async Task<PaginatedResultDto<UserDto>> GetAllUsersAsync(UserQueryDto query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        // TODO: implement in IUserRepository
        // For now, simplified version:
        var allUsers = await _userRepository.GetAllAsync();

        // Apply filtering
        var filteredUsers = allUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            filteredUsers = filteredUsers.Where(u =>
                u.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (query.Role.HasValue)
        {
            filteredUsers = filteredUsers.Where(u => u.Role == query.Role.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            var sortParts = query.SortBy.Split(':');
            var field = sortParts[0].ToLower();
            var direction = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

            filteredUsers = field switch
            {
                "name" => direction == "desc"
                    ? filteredUsers.OrderByDescending(u => u.Name)
                    : filteredUsers.OrderBy(u => u.Name),
                "email" => direction == "desc"
                    ? filteredUsers.OrderByDescending(u => u.Email)
                    : filteredUsers.OrderBy(u => u.Email),
                "createdat" => direction == "desc"
                    ? filteredUsers.OrderByDescending(u => u.CreatedAt)
                    : filteredUsers.OrderBy(u => u.CreatedAt),
                _ => filteredUsers.OrderBy(u => u.Name)
            };
        }

        var totalResults = filteredUsers.Count();
        var totalPages = (int)Math.Ceiling((double)totalResults / query.Limit);

        // Apply pagination
        var paginatedUsers = filteredUsers
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToList();

        return new PaginatedResultDto<UserDto>
        {
            Results = paginatedUsers.Select(MapToDto),
            Page = query.Page,
            Limit = query.Limit,
            TotalPages = totalPages,
            TotalResults = totalResults
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new UserNotFoundException(id);

        return MapToDto(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
    {
        if (updateDto == null)
            throw new ArgumentNullException(nameof(updateDto));

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new UserNotFoundException(id);

        if (!string.IsNullOrWhiteSpace(updateDto.Email) &&
            updateDto.Email.ToLowerInvariant() != user.Email.ToLowerInvariant())
        {
            if (await _userRepository.IsEmailTakenAsync(updateDto.Email, id))
                throw new EmailAlreadyTakenException(updateDto.Email);

            user.Email = updateDto.Email.ToLowerInvariant();
            user.IsEmailVerified = false; // Reset verification on change
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Name))
            user.Name = updateDto.Name;

        if (!string.IsNullOrWhiteSpace(updateDto.Password))
        {
            // TODO: Validate password strength
            // TODO: Hash password
            if (!PasswordValidator.IsValid(updateDto.Password))
                throw new WeakPasswordException();

            user.Password = updateDto.Password;
        }

        if (updateDto.IsEmailVerified.HasValue)
            user.IsEmailVerified = updateDto.IsEmailVerified.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        return MapToDto(updatedUser);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new UserNotFoundException(id);

        await _userRepository.DeleteAsync(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified
        };
    }
}
