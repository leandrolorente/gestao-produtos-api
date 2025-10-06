namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO de resposta para fornecedor
/// </summary>
public record FornecedorDto
{
    public string Id { get; init; } = string.Empty;
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string CnpjCpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public EnderecoDto? Endereco { get; init; }
    public string? InscricaoEstadual { get; init; }
    public string? InscricaoMunicipal { get; init; }
    public string Tipo { get; init; } = string.Empty; // Nacional, Internacional
    public string Status { get; init; } = string.Empty; // Ativo, Inativo, Bloqueado
    public string? Observacoes { get; init; }
    public string? ContatoPrincipal { get; init; }
    public string? Site { get; init; }
    
    // Dados bancários
    public string? Banco { get; init; }
    public string? Agencia { get; init; }
    public string? Conta { get; init; }
    public string? Pix { get; init; }
    
    // Condições comerciais
    public int PrazoPagamentoPadrao { get; init; }
    public decimal LimiteCredito { get; init; }
    public string? CondicoesPagamento { get; init; }
    
    // Estatísticas
    public int QuantidadeProdutos { get; init; }
    public DateTime? UltimaCompra { get; init; }
    public decimal TotalComprado { get; init; }
    public int QuantidadeCompras { get; init; }
    public decimal TicketMedio { get; init; }
    public bool EhFrequente { get; init; }
    
    // Auditoria
    public DateTime DataCriacao { get; init; }
    public DateTime DataAtualizacao { get; init; }
    public bool Ativo { get; init; }
}