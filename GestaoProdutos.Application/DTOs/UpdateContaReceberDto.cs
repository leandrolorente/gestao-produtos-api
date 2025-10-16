using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para atualização de conta a receber
/// </summary>
public record UpdateContaReceberDto
{
    public string Descricao { get; init; } = string.Empty;
    public string? NotaFiscal { get; init; }
    
    // Valores
    public decimal ValorOriginal { get; init; }
    public decimal Desconto { get; init; } = 0;
    
    // Datas
    public DateTime DataEmissao { get; init; }
    public DateTime DataVencimento { get; init; }
    
    // Recorrência
    public bool EhRecorrente { get; init; } = false;
    public TipoRecorrencia? TipoRecorrencia { get; init; }
    
    // Observações
    public string? Observacoes { get; init; }
    public string? VendedorId { get; init; }
}
