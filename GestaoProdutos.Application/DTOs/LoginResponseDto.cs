namespace GestaoProdutos.Application.DTOs;

public record LoginResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = new UserDto();
}
