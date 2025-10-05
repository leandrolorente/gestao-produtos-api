namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO para resposta do ViaCEP com dados filtrados
/// </summary>
public record ViaCepResponseDto
{
    public string Cep { get; init; } = string.Empty;
    public string Logradouro { get; init; } = string.Empty;
    public string Complemento { get; init; } = string.Empty;
    public string Unidade { get; init; } = string.Empty;
    public string Bairro { get; init; } = string.Empty;
    public string Localidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Regiao { get; init; } = string.Empty;
}