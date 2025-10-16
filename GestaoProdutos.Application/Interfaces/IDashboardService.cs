using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(int count = 5);
    Task<IEnumerable<VendaSummaryDto>> GetRecentSalesAsync(int count = 5);
    Task<decimal> GetRevenueByPeriodAsync(DateTime inicio, DateTime fim);
    Task<int> GetSalesCountByPeriodAsync(DateTime inicio, DateTime fim);
}
