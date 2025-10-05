namespace GestaoProdutos.Application.DTOs;

public record VendaDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string ClienteId { get; init; } = string.Empty;
    public string ClienteNome { get; init; } = string.Empty;
    public string ClienteEmail { get; init; } = string.Empty;
    public IEnumerable<VendaItemDto> Items { get; init; } = new List<VendaItemDto>();
    public decimal Subtotal { get; init; }
    public decimal Desconto { get; init; }
    public decimal Total { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string DataVenda { get; init; } = string.Empty;
    public string? DataVencimento { get; init; }
    public string? VendedorId { get; init; }
    public string? VendedorNome { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string UpdatedAt { get; init; } = string.Empty;
}