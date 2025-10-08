namespace GestaoProdutos.Application.DTOs;

public record UpdateVendaDto
{
    public string Numero { get; init; } = string.Empty;
    public string ClienteId { get; init; } = string.Empty;
    public IEnumerable<VendaItemDto> Items { get; init; } = new List<VendaItemDto>();
    public decimal Desconto { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string? DataVencimento { get; init; }
    public string? VendedorId { get; init; }
    public string? VendedorNome { get; init; }
}
