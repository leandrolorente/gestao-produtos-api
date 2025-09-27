using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> GetAllProdutosAsync();
    Task<ProdutoDto?> GetProdutoByIdAsync(string id);
    Task<ProdutoDto?> GetProdutoBySkuAsync(string sku);
    Task<IEnumerable<ProdutoDto>> GetProdutosComEstoqueBaixoAsync();
    Task<IEnumerable<ProdutoDto>> GetProdutosPorCategoriaAsync(string categoria);
    Task<ProdutoDto> CreateProdutoAsync(CreateProdutoDto dto);
    Task<ProdutoDto> UpdateProdutoAsync(string id, UpdateProdutoDto dto);
    Task<bool> DeleteProdutoAsync(string id);
    Task<bool> UpdateEstoqueAsync(string id, int novaQuantidade);
}

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllClientesAsync();
    Task<ClienteDto?> GetClienteByIdAsync(string id);
    Task<ClienteDto?> GetClienteByCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<ClienteDto>> GetClientesAtivosPorTipoAsync(Domain.Enums.TipoCliente tipo);
    Task<IEnumerable<ClienteDto>> GetClientesComCompraRecenteAsync(int dias = 30);
    Task<ClienteDto> CreateClienteAsync(CreateClienteDto dto);
    Task<ClienteDto> UpdateClienteAsync(string id, UpdateClienteDto dto);
    Task<bool> DeleteClienteAsync(string id);
    Task<bool> ToggleStatusClienteAsync(string id);
    Task<bool> RegistrarCompraAsync(string id);
}

public interface IUsuarioService
{
    Task<IEnumerable<UserDto>> GetAllUsuariosAsync();
    Task<UserDto?> GetUsuarioByIdAsync(string id);
    Task<UserDto?> GetUsuarioByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetUsuariosPorRoleAsync(Domain.Enums.UserRole role);
    Task<UserDto> CreateUsuarioAsync(CreateUsuarioDto dto);
    Task<UserDto> UpdateUsuarioAsync(string id, UpdateUsuarioDto dto);
    Task<bool> DeleteUsuarioAsync(string id);
    Task<bool> ToggleStatusUsuarioAsync(string id);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(int count = 5);
}

// DTOs adicionais
public record CreateUsuarioDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Domain.Enums.UserRole Role { get; init; }
    public string Departamento { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

public record UpdateUsuarioDto
{
    public string Nome { get; init; } = string.Empty;
    public string Departamento { get; init; } = string.Empty;
    public Domain.Enums.UserRole Role { get; init; }
}