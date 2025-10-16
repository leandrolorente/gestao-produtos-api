using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

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
