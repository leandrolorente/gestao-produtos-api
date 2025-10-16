namespace GestaoProdutos.Application.DTOs;

public record CreateVendaDto
{
    public string ClienteId { get; init; } = string.Empty;
    public IEnumerable<CreateVendaItemDto> Items { get; init; } = new List<CreateVendaItemDto>();
    public decimal Desconto { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string? DataVencimento { get; init; }
}
