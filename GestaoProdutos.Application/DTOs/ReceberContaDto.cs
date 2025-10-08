using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para recebimento de conta
/// </summary>
public record ReceberContaDto
{
    public decimal Valor { get; init; }
    public FormaPagamento FormaPagamento { get; init; }
    public DateTime? DataRecebimento { get; init; }
    public string? Observacoes { get; init; }
}
