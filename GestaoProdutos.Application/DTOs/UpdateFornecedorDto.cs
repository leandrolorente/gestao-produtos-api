using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para atualização de fornecedor
/// </summary>
public record UpdateFornecedorDto
{
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public UpdateEnderecoDto? Endereco { get; init; }
    public string? InscricaoEstadual { get; init; }
    public string? InscricaoMunicipal { get; init; }
    public TipoFornecedor Tipo { get; init; } = TipoFornecedor.Nacional;
    public StatusFornecedor Status { get; init; } = StatusFornecedor.Ativo;
    public string? Observacoes { get; init; }
    public string? ContatoPrincipal { get; init; }
    public string? Site { get; init; }
    
    // Dados bancários
    public string? Banco { get; init; }
    public string? Agencia { get; init; }
    public string? Conta { get; init; }
    public string? Pix { get; init; }
    
    // Condições comerciais
    public int PrazoPagamentoPadrao { get; init; } = 30;
    public decimal LimiteCredito { get; init; } = 0;
    public string? CondicoesPagamento { get; init; }
}