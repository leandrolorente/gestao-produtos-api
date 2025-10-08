using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(string id);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<UserResponseDto>> GetUsersByDepartmentAsync(string department);
    Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);
    Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto dto);
    Task<bool> DeactivateUserAsync(string id);
    Task<bool> ActivateUserAsync(string id);
}
