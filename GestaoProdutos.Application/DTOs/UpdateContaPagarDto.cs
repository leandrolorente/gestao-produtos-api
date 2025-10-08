using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para atualização de conta a pagar
/// </summary>
public record UpdateContaPagarDto
{
    public string Descricao { get; init; } = string.Empty;
    public string? NotaFiscal { get; init; }
    
    // Valores
    public decimal ValorOriginal { get; init; }
    public decimal Desconto { get; init; } = 0;
    
    // Datas
    public DateTime DataEmissao { get; init; }
    public DateTime DataVencimento { get; init; }
    
    // Categoria
    public CategoriaConta Categoria { get; init; }
    
    // Recorrência
    public bool EhRecorrente { get; init; } = false;
    public TipoRecorrencia? TipoRecorrencia { get; init; }
    public int? DiasRecorrencia { get; init; }
    
    // Observações
    public string? Observacoes { get; init; }
    public string? CentroCusto { get; init; }
}
