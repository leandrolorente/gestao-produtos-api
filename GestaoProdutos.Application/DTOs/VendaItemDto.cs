namespace GestaoProdutos.Application.DTOs;

public record VendaItemDto
{
    public string Id { get; init; } = string.Empty;
    public string ProdutoId { get; init; } = string.Empty;
    public string ProdutoNome { get; init; } = string.Empty;
    public string ProdutoSku { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal PrecoUnitario { get; init; }
    public decimal Subtotal { get; init; }
}
