namespace GestaoProdutos.Application.DTOs;

public record CreateProdutoDto
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; } // Código de barras
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string? Categoria { get; init; }
    public string? Descricao { get; init; }
    public decimal? PrecoCompra { get; init; }
    public int? EstoqueMinimo { get; init; }
}
