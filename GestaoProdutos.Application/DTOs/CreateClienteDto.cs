using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

public record CreateClienteDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public CreateEnderecoDto Endereco { get; init; } = new();
    public TipoCliente Tipo { get; init; }
    public string? Observacoes { get; init; }
}
