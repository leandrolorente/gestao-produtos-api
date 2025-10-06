namespace GestaoProdutos.Application.DTOs;

public record ForgotPasswordDto
{
    public string Email { get; init; } = string.Empty;
}