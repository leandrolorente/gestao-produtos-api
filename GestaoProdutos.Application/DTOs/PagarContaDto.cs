using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para pagamento de conta
/// </summary>
public record PagarContaDto
{
    public decimal Valor { get; init; }
    public FormaPagamento FormaPagamento { get; init; }
    public DateTime? DataPagamento { get; init; }
    public string? Observacoes { get; init; }
}
