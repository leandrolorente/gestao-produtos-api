using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para criação de conta a receber
/// </summary>
public record CreateContaReceberDto
{
    public string Descricao { get; init; } = string.Empty;
    public string? ClienteId { get; init; }
    public string? VendaId { get; init; }
    public string? NotaFiscal { get; init; }
    
    // Valores
    public decimal ValorOriginal { get; init; }
    public decimal Desconto { get; init; } = 0;
    
    // Datas
    public DateTime DataEmissao { get; init; } = DateTime.UtcNow;
    public DateTime DataVencimento { get; init; }
    
    // Recorrência
    public bool EhRecorrente { get; init; } = false;
    public TipoRecorrencia? TipoRecorrencia { get; init; }
    
    // Observações
    public string? Observacoes { get; init; }
    public string? VendedorId { get; init; }
}
