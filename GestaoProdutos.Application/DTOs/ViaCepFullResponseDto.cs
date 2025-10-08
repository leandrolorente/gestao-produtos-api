namespace GestaoProdutos.Application.DTOs;

/// <summary>
/// DTO interno para resposta completa do ViaCEP
/// </summary>
internal record ViaCepFullResponseDto
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
    public string Ibge { get; init; } = string.Empty;
    public string Gia { get; init; } = string.Empty;
    public string Ddd { get; init; } = string.Empty;
    public string Siafi { get; init; } = string.Empty;
    public bool Erro { get; init; } = false;
}
