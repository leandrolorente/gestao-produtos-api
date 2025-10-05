namespace GestaoProdutos.Application.DTOs;

public record ClienteDto
{
    public string Id { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public EnderecoDto? Endereco { get; init; }
    public string Tipo { get; init; } = string.Empty; // "Pessoa Física" ou "Pessoa Jurídica"
    public bool Ativo { get; init; }
    public DateTime DataCadastro { get; init; }
    public DateTime? UltimaCompra { get; init; }
    public string? Observacoes { get; init; }
}