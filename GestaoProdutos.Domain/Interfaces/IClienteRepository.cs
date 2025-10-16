using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetClientePorCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<Cliente>> GetClientesAtivosPorTipoAsync(TipoCliente tipo);
    Task<IEnumerable<Cliente>> GetClientesComCompraRecenteAsync(int dias = 30);
    Task<bool> CpfCnpjJaExisteAsync(string cpfCnpj, string? clienteId = null);
}
