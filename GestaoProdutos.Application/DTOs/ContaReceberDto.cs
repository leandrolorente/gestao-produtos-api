using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO de resposta para conta a receber
/// </summary>
public record ContaReceberDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string? ClienteId { get; init; }
    public string? ClienteNome { get; init; }
    public string? VendaId { get; init; }
    public string? NotaFiscal { get; init; }
    
    // Valores
    public decimal ValorOriginal { get; init; }
    public decimal Desconto { get; init; }
    public decimal Juros { get; init; }
    public decimal Multa { get; init; }
    public decimal ValorRecebido { get; init; }
    public decimal ValorRestante { get; init; }
    
    // Datas
    public DateTime DataEmissao { get; init; }
    public DateTime DataVencimento { get; init; }
    public DateTime? DataRecebimento { get; init; }
    
    // Status
    public string Status { get; init; } = string.Empty;
    public string? FormaPagamento { get; init; }
    
    // Recorrência
    public bool EhRecorrente { get; init; }
    public string? TipoRecorrencia { get; init; }
    
    // Observações
    public string? Observacoes { get; init; }
    public string? VendedorId { get; init; }
    public string? VendedorNome { get; init; }
    
    // Propriedades calculadas
    public bool EstaVencida { get; init; }
    public int DiasVencimento { get; init; }
    
    // Auditoria
    public DateTime DataCriacao { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public bool Ativo { get; init; }
}
