using GestaoProdutos.Domain.Enums;
using System.Text.Json.Serialization;

namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para criação de fornecedor
/// </summary>
public record CreateFornecedorDto
{
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string CnpjCpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public CreateEnderecoDto? Endereco { get; init; }
    public string? InscricaoEstadual { get; init; }
    public string? InscricaoMunicipal { get; init; }
    public TipoFornecedor Tipo { get; init; }
    public StatusFornecedor? Status { get; init; } // Opcional para criação, padrão será Ativo
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
}
