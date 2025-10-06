namespace GestaoProdutos.Application.DTOs;

public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty; // "admin", "manager", "user"
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public DateTime? LastUpdated { get; init; }
    public bool IsActive { get; init; }
}