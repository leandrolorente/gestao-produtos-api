namespace GestaoProdutos.Application.DTOs;

public record EnderecoDto
{
    public string Id { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string Logradouro { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string? Complemento { get; init; }
    public string Unidade { get; init; } = string.Empty;
    public string Bairro { get; init; } = string.Empty;
    public string Localidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Regiao { get; init; } = string.Empty;
    public string? Referencia { get; init; }
    public bool IsPrincipal { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime DataAtualizacao { get; init; }
}
