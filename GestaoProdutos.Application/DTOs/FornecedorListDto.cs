namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para listagem simplificada de fornecedores
/// </summary>
public record FornecedorListDto
{
    public string Id { get; init; } = string.Empty;
    public string RazaoSocial { get; init; } = string.Empty;
    public string? NomeFantasia { get; init; }
    public string CnpjCpf { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ContatoPrincipal { get; init; }
    public DateTime? UltimaCompra { get; init; }
    public decimal TotalComprado { get; init; }
    public int QuantidadeCompras { get; init; }
    public bool EhFrequente { get; init; }
    public bool Ativo { get; init; }
}