namespace GestaoProdutos.Application.DTOs;

public record TopClienteDto
{
    public string ClienteNome { get; init; } = string.Empty;
    public int TotalCompras { get; init; }
    public decimal ValorTotal { get; init; }
}