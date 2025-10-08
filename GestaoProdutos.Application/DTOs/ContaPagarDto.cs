using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO de resposta para conta a pagar
/// </summary>
public record ContaPagarDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string? FornecedorId { get; init; }
    public string? FornecedorNome { get; init; }
    public string? CompraId { get; init; }
    public string? NotaFiscal { get; init; }
    
    // Valores
    public decimal ValorOriginal { get; init; }
    public decimal Desconto { get; init; }
    public decimal Juros { get; init; }
    public decimal Multa { get; init; }
    public decimal ValorPago { get; init; }
    public decimal ValorRestante { get; init; }
    
    // Datas
    public DateTime DataEmissao { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataPagamento { get; init; }
    
    // Status e categoria
    public string Status { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string? FormaPagamento { get; init; }
    
    // Recorrência
    public bool EhRecorrente { get; init; }
    public string? TipoRecorrencia { get; init; }
    public int? DiasRecorrencia { get; init; }
    
    // Observações
    public string? Observacoes { get; init; }
    public string? CentroCusto { get; init; }
    
    // Propriedades calculadas
    public bool EstaVencida { get; init; }
    public int DiasVencimento { get; init; }
    
    // Auditoria
    public DateTime DataCriacao { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public bool Ativo { get; init; }
}
