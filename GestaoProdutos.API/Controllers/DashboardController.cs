using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller responsável pelas estatísticas e dados do dashboard
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Obter estatísticas completas do dashboard
    /// </summary>
    /// <returns>Estatísticas consolidadas do sistema</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Obter produtos mais vendidos
    /// </summary>
    /// <param name="count">Quantidade de produtos a retornar (padrão: 5)</param>
    /// <returns>Lista dos produtos mais vendidos</returns>
    [HttpGet("top-products")]
    public async Task<ActionResult<IEnumerable<ProductSummaryDto>>> GetTopSellingProducts([FromQuery] int count = 5)
    {
        try
        {
            if (count <= 0 || count > 50)
                count = 5;

            var products = await _dashboardService.GetTopSellingProductsAsync(count);
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Obter vendas recentes
    /// </summary>
    /// <param name="count">Quantidade de vendas a retornar (padrão: 5)</param>
    /// <returns>Lista das vendas mais recentes</returns>
    [HttpGet("recent-sales")]
    public async Task<ActionResult<IEnumerable<VendaSummaryDto>>> GetRecentSales([FromQuery] int count = 5)
    {
        try
        {
            if (count <= 0 || count > 50)
                count = 5;

            var sales = await _dashboardService.GetRecentSalesAsync(count);
            return Ok(sales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Obter receita por período
    /// </summary>
    /// <param name="inicio">Data inicial (formato: yyyy-MM-dd)</param>
    /// <param name="fim">Data final (formato: yyyy-MM-dd)</param>
    /// <returns>Receita total do período</returns>
    [HttpGet("revenue")]
    public async Task<ActionResult<object>> GetRevenueByPeriod(
        [FromQuery] DateTime inicio, 
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
            {
                return BadRequest(new { message = "Data inicial não pode ser maior que data final" });
            }

            if ((fim - inicio).TotalDays > 365)
            {
                return BadRequest(new { message = "Período não pode ser maior que 1 ano" });
            }

            var revenue = await _dashboardService.GetRevenueByPeriodAsync(inicio, fim);
            var salesCount = await _dashboardService.GetSalesCountByPeriodAsync(inicio, fim);

            return Ok(new 
            { 
                periodo = new { inicio, fim },
                receita = revenue,
                totalVendas = salesCount,
                receitaMedia = salesCount > 0 ? revenue / salesCount : 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Obter estatísticas rápidas para widgets
    /// </summary>
    /// <returns>Dados essenciais para widgets do dashboard</returns>
    [HttpGet("widgets")]
    public async Task<ActionResult<object>> GetWidgetsData()
    {
        try
        {
            var hoje = DateTime.Today;
            var ontem = hoje.AddDays(-1);
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var stats = await _dashboardService.GetDashboardStatsAsync();
            var receitaOntem = await _dashboardService.GetRevenueByPeriodAsync(ontem, ontem);
            var receitaMes = await _dashboardService.GetRevenueByPeriodAsync(inicioMes, hoje);

            // Calcular crescimento em relação a ontem
            var crescimentoDiario = receitaOntem > 0 
                ? ((stats.RevenueToday - receitaOntem) / receitaOntem) * 100 
                : 0;

            return Ok(new 
            {
                // Vendas
                vendas = new 
                {
                    total = stats.TotalSales,
                    hoje = stats.SalesToday,
                    pendentes = stats.PendingSales
                },
                
                // Receita
                receita = new 
                {
                    total = stats.TotalRevenue,
                    hoje = stats.RevenueToday,
                    mes = receitaMes,
                    crescimentoDiario = Math.Round(crescimentoDiario, 2)
                },
                
                // Produtos
                produtos = new 
                {
                    total = stats.TotalProducts,
                    estoqueBaixo = stats.LowStockProducts,
                    valorTotal = stats.TotalValue
                },
                
                // Clientes
                clientes = new 
                {
                    total = stats.TotalClients,
                    ativos = stats.ActiveClients
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor",
                details = ex.Message 
            });
        }
    }
}
