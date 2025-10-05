using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

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