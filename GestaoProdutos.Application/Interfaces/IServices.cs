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

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(string id);
    Task<UserResponseDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UserResponseDto>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<UserResponseDto>> GetUsersByDepartmentAsync(string department);
    Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);
    Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto dto);
    Task<bool> DeactivateUserAsync(string id);
    Task<bool> ActivateUserAsync(string id);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(int count = 5);
}

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
}