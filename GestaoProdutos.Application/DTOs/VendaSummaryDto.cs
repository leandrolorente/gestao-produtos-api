namespace GestaoProdutos.Application.DTOs;

public record VendaSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string ClienteNome { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime DataVenda { get; init; }
}