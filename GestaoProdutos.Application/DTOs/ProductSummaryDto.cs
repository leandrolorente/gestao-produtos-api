namespace GestaoProdutos.Application.DTOs;

public record ProductSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int Sales { get; init; }
    public decimal Revenue { get; init; }
}
