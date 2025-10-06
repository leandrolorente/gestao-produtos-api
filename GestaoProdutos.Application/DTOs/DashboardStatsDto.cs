namespace GestaoProdutos.Application.DTOs;

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