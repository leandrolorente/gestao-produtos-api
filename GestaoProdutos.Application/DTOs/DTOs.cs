using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.DTOs;

// DTOs baseados no frontend Angular
public record ProdutoDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; } // Código de barras
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public DateTime LastUpdated { get; init; }
    public string? Categoria { get; init; }
    public bool EstoqueBaixo { get; init; }
}

public record CreateProdutoDto
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; } // Código de barras
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string? Categoria { get; init; }
    public string? Descricao { get; init; }
    public decimal? PrecoCompra { get; init; }
    public int? EstoqueMinimo { get; init; }
}

public record UpdateProdutoDto
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty; // SKU agora editável
    public string? Barcode { get; init; } // Código de barras
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public string? Categoria { get; init; }
    public string? Descricao { get; init; }
    public decimal? PrecoCompra { get; init; }
    public int? EstoqueMinimo { get; init; }
}

public record ClienteDto
{
    public string Id { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public string Endereco { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty; // "Pessoa Física" ou "Pessoa Jurídica"
    public bool Ativo { get; init; }
    public DateTime DataCadastro { get; init; }
    public DateTime? UltimaCompra { get; init; }
    public string? Observacoes { get; init; }
}

public record CreateClienteDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public string Endereco { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public TipoCliente Tipo { get; init; }
    public string? Observacoes { get; init; }
}

public record UpdateClienteDto
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string CpfCnpj { get; init; } = string.Empty;
    public string Endereco { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
}

public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty; // "admin", "manager", "user"
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public DateTime? LastUpdated { get; init; }
    public bool IsActive { get; init; }
}

public record DashboardStatsDto
{
    // Produtos
    public int TotalProducts { get; init; }
    public int LowStockProducts { get; init; }
    public decimal TotalValue { get; init; }
    
    // Vendas
    public int TotalSales { get; init; }
    public decimal TotalRevenue { get; init; }
    public int SalesToday { get; init; }
    public decimal RevenueToday { get; init; }
    public int PendingSales { get; init; }
    
    // Clientes
    public int TotalClients { get; init; }
    public int ActiveClients { get; init; }
    
    // Transações recentes (mantido para compatibilidade)
    public int RecentTransactions { get; init; }
    public int PendingOrders { get; init; }
    
    // Top produtos
    public IEnumerable<ProductSummaryDto> TopSellingProducts { get; init; } = new List<ProductSummaryDto>();
    public IEnumerable<VendaSummaryDto> RecentSales { get; init; } = new List<VendaSummaryDto>();
}

public record ProductSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int Sales { get; init; }
    public decimal Revenue { get; init; }
}

public record VendaSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string ClienteNome { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime DataVenda { get; init; }
}

// ===== AUTHENTICATION DTOs =====
public record LoginDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = new UserDto();
}

public record LogoutDto
{
    public string UserId { get; init; } = string.Empty;
    public string? Token { get; init; } // Token atual (opcional para validação)
    public string? SessionId { get; init; } // ID da sessão (opcional)
    public string? DeviceInfo { get; init; } // Informações do dispositivo (opcional)
}

public record RegisterDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
}

public record ForgotPasswordDto
{
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordDto
{
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record ChangePasswordDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

// ===== USER MANAGEMENT DTOs =====
public record UserCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Role { get; init; } = "user"; // Default role
}

public record UserResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsActive { get; init; }
}

public record UpdateUserDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

// ===== SALES DTOs =====
public record VendaItemDto
{
    public string Id { get; init; } = string.Empty;
    public string ProdutoId { get; init; } = string.Empty;
    public string ProdutoNome { get; init; } = string.Empty;
    public string ProdutoSku { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal PrecoUnitario { get; init; }
    public decimal Subtotal { get; init; }
}

public record VendaDto
{
    public string Id { get; init; } = string.Empty;
    public string Numero { get; init; } = string.Empty;
    public string ClienteId { get; init; } = string.Empty;
    public string ClienteNome { get; init; } = string.Empty;
    public string ClienteEmail { get; init; } = string.Empty;
    public IEnumerable<VendaItemDto> Items { get; init; } = new List<VendaItemDto>();
    public decimal Subtotal { get; init; }
    public decimal Desconto { get; init; }
    public decimal Total { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string DataVenda { get; init; } = string.Empty;
    public string? DataVencimento { get; init; }
    public string? VendedorId { get; init; }
    public string? VendedorNome { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public string UpdatedAt { get; init; } = string.Empty;
}

public record CreateVendaDto
{
    public string ClienteId { get; init; } = string.Empty;
    public IEnumerable<CreateVendaItemDto> Items { get; init; } = new List<CreateVendaItemDto>();
    public decimal Desconto { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string? DataVencimento { get; init; }
}

public record CreateVendaItemDto
{
    public string ProdutoId { get; init; } = string.Empty;
    public string ProdutoNome { get; init; } = string.Empty;
    public string ProdutoSku { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal PrecoUnitario { get; init; }
    public decimal Subtotal { get; init; }
}

public record UpdateVendaDto
{
    public string Numero { get; init; } = string.Empty;
    public string ClienteId { get; init; } = string.Empty;
    public IEnumerable<VendaItemDto> Items { get; init; } = new List<VendaItemDto>();
    public decimal Desconto { get; init; }
    public string FormaPagamento { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public string? DataVencimento { get; init; }
    public string? VendedorId { get; init; }
    public string? VendedorNome { get; init; }
}

public record VendasStatsDto
{
    public int TotalVendas { get; init; }
    public int VendasHoje { get; init; }
    public decimal FaturamentoMes { get; init; }
    public decimal TicketMedio { get; init; }
    public int VendasPendentes { get; init; }
    public IEnumerable<TopClienteDto> TopClientes { get; init; } = new List<TopClienteDto>();
    public IEnumerable<VendasPorMesDto> VendasPorMes { get; init; } = new List<VendasPorMesDto>();
}

public record TopClienteDto
{
    public string ClienteNome { get; init; } = string.Empty;
    public int TotalCompras { get; init; }
    public decimal ValorTotal { get; init; }
}

public record VendasPorMesDto
{
    public string Mes { get; init; } = string.Empty;
    public int Vendas { get; init; }
    public decimal Faturamento { get; init; }
}

// ViaCEP DTOs
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