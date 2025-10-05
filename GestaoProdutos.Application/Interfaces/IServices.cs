using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> GetAllProdutosAsync();
    Task<ProdutoDto?> GetProdutoByIdAsync(string id);
    Task<ProdutoDto?> GetProdutoBySkuAsync(string sku);
    Task<ProdutoDto?> GetProdutoByBarcodeAsync(string barcode);
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

public interface IEnderecoService
{
    Task<IEnumerable<EnderecoDto>> GetAllAsync();
    Task<EnderecoDto?> GetByIdAsync(string id);
    Task<EnderecoDto?> GetByCepAsync(string cep);
    Task<IEnumerable<EnderecoDto>> GetByCidadeAsync(string cidade);
    Task<IEnumerable<EnderecoDto>> GetByEstadoAsync(string estado);
    Task<EnderecoDto> CreateAsync(CreateEnderecoDto createDto);
    Task<EnderecoDto?> UpdateAsync(string id, UpdateEnderecoDto updateDto);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<EnderecoDto>> SearchAsync(string termo);
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
    Task<IEnumerable<VendaSummaryDto>> GetRecentSalesAsync(int count = 5);
    Task<decimal> GetRevenueByPeriodAsync(DateTime inicio, DateTime fim);
    Task<int> GetSalesCountByPeriodAsync(DateTime inicio, DateTime fim);
}

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<bool> LogoutAsync(LogoutDto logoutDto);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    Task<UserDto> GetCurrentUserAsync(string userId);
    Task<bool> ValidateTokenAsync(string token);
}

public interface IVendaService
{
    Task<IEnumerable<VendaDto>> GetAllVendasAsync();
    Task<VendaDto?> GetVendaByIdAsync(string id);
    Task<VendaDto?> GetVendaByNumeroAsync(string numero);
    Task<IEnumerable<VendaDto>> GetVendasPorClienteAsync(string clienteId);
    Task<IEnumerable<VendaDto>> GetVendasPorVendedorAsync(string vendedorId);
    Task<IEnumerable<VendaDto>> GetVendasPorStatusAsync(string status);
    Task<IEnumerable<VendaDto>> GetVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<VendaDto>> GetVendasVencidasAsync();
    Task<IEnumerable<VendaDto>> GetVendasHojeAsync();
    Task<VendaDto> CreateVendaAsync(CreateVendaDto dto, string? vendedorId = null);
    Task<VendaDto> UpdateVendaAsync(string id, UpdateVendaDto dto);
    Task<bool> DeleteVendaAsync(string id);
    Task<VendaDto> ConfirmarVendaAsync(string id);
    Task<VendaDto> FinalizarVendaAsync(string id);
    Task<VendaDto> CancelarVendaAsync(string id);
    Task<VendasStatsDto> GetVendasStatsAsync();
    Task<string> GetProximoNumeroVendaAsync();
}

/// <summary>
/// Interface para serviço de fornecedores
/// </summary>
public interface IFornecedorService
{
    Task<IEnumerable<FornecedorDto>> GetAllFornecedoresAsync();
    Task<IEnumerable<FornecedorListDto>> GetAllFornecedoresListAsync();
    Task<FornecedorDto?> GetFornecedorByIdAsync(string id);
    Task<FornecedorDto?> GetFornecedorByCnpjCpfAsync(string cnpjCpf);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresAtivosPorTipoAsync(Domain.Enums.TipoFornecedor tipo);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresComCompraRecenteAsync(int dias = 90);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresPorStatusAsync(Domain.Enums.StatusFornecedor status);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresPorProdutoAsync(string produtoId);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresFrequentesAsync();
    Task<IEnumerable<FornecedorDto>> BuscarFornecedoresAsync(string termo);
    Task<FornecedorDto> CreateFornecedorAsync(CreateFornecedorDto dto);
    Task<FornecedorDto> UpdateFornecedorAsync(string id, UpdateFornecedorDto dto);
    Task<bool> DeleteFornecedorAsync(string id);
    Task<bool> BloquearFornecedorAsync(string id, string? motivo);
    Task<bool> DesbloquearFornecedorAsync(string id);
    Task<bool> InativarFornecedorAsync(string id);
    Task<bool> AtivarFornecedorAsync(string id);
    Task<bool> AdicionarProdutoAsync(string fornecedorId, string produtoId);
    Task<bool> RemoverProdutoAsync(string fornecedorId, string produtoId);
    Task<bool> RegistrarCompraAsync(string fornecedorId, decimal valor);
    Task<bool> AtualizarCondicoesComerciais(string fornecedorId, int prazoPagamento, decimal limiteCredito, string? condicoesPagamento);
    Task<bool> AtualizarDadosBancarios(string fornecedorId, string? banco, string? agencia, string? conta, string? pix);
}

/// <summary>
/// Interface para serviço de integração com ViaCEP
/// </summary>
public interface IViaCepService
{
    /// <summary>
    /// Busca informações de endereço pelo CEP
    /// </summary>
    /// <param name="cep">CEP para consulta (formato: 12345678 ou 12345-678)</param>
    /// <returns>Dados do endereço ou null se não encontrado</returns>
    Task<ViaCepResponseDto?> BuscarEnderecoPorCepAsync(string cep);
}