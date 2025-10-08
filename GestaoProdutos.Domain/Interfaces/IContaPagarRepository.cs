using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

/// <summary>
/// Interface para repositório de contas a pagar
/// </summary>
public interface IContaPagarRepository
{
    Task<ContaPagar?> GetByIdAsync(string id);
    Task<IEnumerable<ContaPagar>> GetAllAsync();
    Task<IEnumerable<ContaPagar>> GetByStatusAsync(StatusContaPagar status);
    Task<IEnumerable<ContaPagar>> GetVencidasAsync();
    Task<IEnumerable<ContaPagar>> GetByFornecedorAsync(string fornecedorId);
    Task<IEnumerable<ContaPagar>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<ContaPagar>> GetVencendoEmAsync(int dias);
    Task<IEnumerable<ContaPagar>> GetByCategoriaAsync(CategoriaConta categoria);
    Task<ContaPagar> CreateAsync(ContaPagar conta);
    Task<ContaPagar> UpdateAsync(ContaPagar conta);
    Task<bool> DeleteAsync(string id);
    Task<string> GetProximoNumeroAsync();
    Task<decimal> GetTotalPagarPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<decimal> GetTotalPagoPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<int> GetQuantidadeVencidasAsync();
    Task<IEnumerable<ContaPagar>> GetRecorrentesParaGerarAsync();
}
