using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Interfaces;

namespace GestaoProdutos.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            // Buscar dados de produtos
            var produtos = await _unitOfWork.Produtos.GetAllAsync();
            var produtosAtivos = produtos.Where(p => p.Ativo).ToList();
            
            // Buscar dados de vendas
            var vendas = await _unitOfWork.Vendas.GetAllAsync();
            var vendasAtivas = vendas.Where(v => v.Ativo).ToList();
            
            // Buscar dados de clientes
            var clientes = await _unitOfWork.Clientes.GetAllAsync();
            var clientesAtivos = clientes.Where(c => c.Ativo).ToList();

            // Calcular estatísticas de produtos
            var totalProducts = produtosAtivos.Count;
            var lowStockProducts = produtosAtivos.Count(p => p.EstaComEstoqueBaixo());
            var totalValue = produtosAtivos.Sum(p => p.Preco * p.Quantidade);

            // Calcular estatísticas de vendas
            var totalSales = vendasAtivas.Count;
            var totalRevenue = vendasAtivas.Where(v => v.Status != Domain.Enums.StatusVenda.Cancelada)
                                         .Sum(v => v.Total);

            // Vendas de hoje
            var hoje = DateTime.Today;
            var vendasHoje = vendasAtivas.Where(v => v.DataVenda.Date == hoje).ToList();
            var salesToday = vendasHoje.Count;
            var revenueToday = vendasHoje.Where(v => v.Status != Domain.Enums.StatusVenda.Cancelada)
                                       .Sum(v => v.Total);

            // Vendas pendentes
            var pendingSales = vendasAtivas.Count(v => v.Status == Domain.Enums.StatusVenda.Pendente);

            // Clientes
            var totalClients = clientesAtivos.Count;
            var activeClients = clientesAtivos.Count(c => c.UltimaCompra.HasValue && 
                                                        c.UltimaCompra.Value > DateTime.Now.AddDays(-30));

            // Top produtos mais vendidos
            var topSellingProducts = await GetTopSellingProductsAsync(5);
            
            // Vendas recentes
            var recentSales = await GetRecentSalesAsync(5);

            return new DashboardStatsDto
            {
                // Produtos
                TotalProducts = totalProducts,
                LowStockProducts = lowStockProducts,
                TotalValue = totalValue,
                
                // Vendas
                TotalSales = totalSales,
                TotalRevenue = totalRevenue,
                SalesToday = salesToday,
                RevenueToday = revenueToday,
                PendingSales = pendingSales,
                
                // Clientes
                TotalClients = totalClients,
                ActiveClients = activeClients,
                
                // Compatibilidade
                RecentTransactions = salesToday,
                PendingOrders = pendingSales,
                
                // Lists
                TopSellingProducts = topSellingProducts,
                RecentSales = recentSales
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter estatísticas do dashboard: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(int count = 5)
    {
        try
        {
            var vendas = await _unitOfWork.Vendas.GetAllAsync();
            var vendasFinalizadas = vendas.Where(v => v.Status == Domain.Enums.StatusVenda.Finalizada && v.Ativo);

            // Agrupar por produto e somar quantidades vendidas
            var produtoVendas = vendasFinalizadas
                .SelectMany(v => v.Items)
                .GroupBy(item => item.ProdutoId)
                .Select(g => new
                {
                    ProdutoId = g.Key,
                    QuantidadeVendida = g.Sum(item => item.Quantidade),
                    ReceitaTotal = g.Sum(item => item.Subtotal)
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .Take(count);

            var result = new List<ProductSummaryDto>();
            
            foreach (var produtoVenda in produtoVendas)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(produtoVenda.ProdutoId);
                if (produto != null && produto.Ativo)
                {
                    result.Add(new ProductSummaryDto
                    {
                        Id = produto.Id,
                        Name = produto.Nome,
                        Quantity = produto.Quantidade,
                        Sales = produtoVenda.QuantidadeVendida,
                        Revenue = produtoVenda.ReceitaTotal
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter produtos mais vendidos: {ex.Message}");
        }
    }

    public async Task<IEnumerable<VendaSummaryDto>> GetRecentSalesAsync(int count = 5)
    {
        try
        {
            var vendas = await _unitOfWork.Vendas.GetAllAsync();
            
            var vendasRecentes = vendas
                .Where(v => v.Ativo)
                .OrderByDescending(v => v.DataVenda)
                .Take(count);

            var result = new List<VendaSummaryDto>();

            foreach (var venda in vendasRecentes)
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(venda.ClienteId);
                
                result.Add(new VendaSummaryDto
                {
                    Id = venda.Id,
                    Numero = venda.Numero,
                    ClienteNome = cliente?.Nome ?? "Cliente não encontrado",
                    Total = venda.Total,
                    Status = venda.Status.ToString(),
                    DataVenda = venda.DataVenda
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao obter vendas recentes: {ex.Message}");
        }
    }

    public async Task<decimal> GetRevenueByPeriodAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            var vendas = await _unitOfWork.Vendas.GetAllAsync();
            
            return vendas
                .Where(v => v.Ativo && 
                           v.Status != Domain.Enums.StatusVenda.Cancelada &&
                           v.DataVenda.Date >= inicio.Date && 
                           v.DataVenda.Date <= fim.Date)
                .Sum(v => v.Total);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao calcular receita do período: {ex.Message}");
        }
    }

    public async Task<int> GetSalesCountByPeriodAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            var vendas = await _unitOfWork.Vendas.GetAllAsync();
            
            return vendas
                .Count(v => v.Ativo && 
                           v.Status != Domain.Enums.StatusVenda.Cancelada &&
                           v.DataVenda.Date >= inicio.Date && 
                           v.DataVenda.Date <= fim.Date);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao contar vendas do período: {ex.Message}");
        }
    }
}