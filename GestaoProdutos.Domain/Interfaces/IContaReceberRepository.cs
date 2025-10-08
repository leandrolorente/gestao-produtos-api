using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

/// <summary>
/// Interface para repositório de contas a receber
/// </summary>
public interface IContaReceberRepository
{
    Task<ContaReceber?> GetByIdAsync(string id);
    Task<IEnumerable<ContaReceber>> GetAllAsync();
    Task<IEnumerable<ContaReceber>> GetByStatusAsync(StatusContaReceber status);
    Task<IEnumerable<ContaReceber>> GetVencidasAsync();
    Task<IEnumerable<ContaReceber>> GetByClienteAsync(string clienteId);
    Task<IEnumerable<ContaReceber>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<ContaReceber>> GetVencendoEmAsync(int dias);
    Task<ContaReceber> CreateAsync(ContaReceber conta);
    Task<ContaReceber> UpdateAsync(ContaReceber conta);
    Task<bool> DeleteAsync(string id);
    Task<string> GetProximoNumeroAsync();
    Task<decimal> GetTotalReceberPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<decimal> GetTotalRecebidoPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<int> GetQuantidadeVencidasAsync();
    Task<IEnumerable<ContaReceber>> GetRecorrentesParaGerarAsync();
    Task<IEnumerable<ContaReceber>> GetByVendedorAsync(string vendedorId);
}
