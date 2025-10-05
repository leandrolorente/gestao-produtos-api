namespace GestaoProdutos.Application.DTOs;

public record CreateEnderecoDto
{
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
    public bool IsPrincipal { get; init; } = true;
    public string Tipo { get; init; } = "Residencial";
}