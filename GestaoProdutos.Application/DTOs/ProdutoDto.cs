namespace GestaoProdutos.Application.DTOs;

// DTOs baseados no frontend Angular
public record ProdutoDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; } // Código de barras
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public DateTime LastUpdated { get; init; }
    public string? Categoria { get; init; }
    public bool EstoqueBaixo { get; init; }
}