namespace GestaoProdutos.Application.DTOs;

public record LogoutDto
{
    public string UserId { get; init; } = string.Empty;
    public string? Token { get; init; } // Token atual (opcional para validação)
    public string? SessionId { get; init; } // ID da sessão (opcional)
    public string? DeviceInfo { get; init; } // Informações do dispositivo (opcional)
}