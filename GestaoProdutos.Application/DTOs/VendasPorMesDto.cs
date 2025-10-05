namespace GestaoProdutos.Application.DTOs;

public record VendasPorMesDto
{
    public string Mes { get; init; } = string.Empty;
    public int Vendas { get; init; }
    public decimal Faturamento { get; init; }
}