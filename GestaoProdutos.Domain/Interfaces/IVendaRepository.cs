using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

public interface IVendaRepository : IRepository<Venda>
{
    Task<Venda?> GetVendaPorNumeroAsync(string numero);
    Task<IEnumerable<Venda>> GetVendasPorClienteAsync(string clienteId);
    Task<IEnumerable<Venda>> GetVendasPorVendedorAsync(string vendedorId);
    Task<IEnumerable<Venda>> GetVendasPorStatusAsync(StatusVenda status);
    Task<IEnumerable<Venda>> GetVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Venda>> GetVendasVencidasAsync();
    Task<IEnumerable<Venda>> GetVendasHojeAsync();
    Task<decimal> GetFaturamentoPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<string> GetProximoNumeroVendaAsync();
    Task<bool> NumeroVendaJaExisteAsync(string numero, string? vendaId = null);
    Task<IEnumerable<TopClienteResult>> GetTopClientesAsync(int quantidade = 10);
    Task<IEnumerable<VendasPorMesResult>> GetVendasPorMesAsync(int meses = 12);
}
