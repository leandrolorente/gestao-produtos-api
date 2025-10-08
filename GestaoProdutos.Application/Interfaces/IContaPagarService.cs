using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.Interfaces;

/// <summary>
/// Interface para serviços de contas a pagar
/// </summary>
public interface IContaPagarService
{
    Task<IEnumerable<ContaPagarDto>> GetAllContasPagarAsync();
    Task<ContaPagarDto?> GetContaPagarByIdAsync(string id);
    Task<IEnumerable<ContaPagarDto>> GetContasPagarByStatusAsync(StatusContaPagar status);
    Task<IEnumerable<ContaPagarDto>> GetContasPagarVencidasAsync();
    Task<IEnumerable<ContaPagarDto>> GetContasPagarByFornecedorAsync(string fornecedorId);
    Task<IEnumerable<ContaPagarDto>> GetContasPagarByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<ContaPagarDto>> GetContasPagarVencendoEmAsync(int dias);
    Task<IEnumerable<ContaPagarDto>> GetContasPagarByCategoriaAsync(CategoriaConta categoria);
    Task<ContaPagarDto> CreateContaPagarAsync(CreateContaPagarDto dto);
    Task<ContaPagarDto> UpdateContaPagarAsync(string id, UpdateContaPagarDto dto);
    Task<bool> DeleteContaPagarAsync(string id);
    Task<ContaPagarDto> PagarContaAsync(string id, PagarContaDto dto);
    Task<bool> CancelarContaAsync(string id);
    Task<decimal> GetTotalPagarPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<decimal> GetTotalPagoPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<int> GetQuantidadeContasVencidasAsync();
    Task AtualizarStatusContasAsync();
    Task ProcessarContasRecorrentesAsync();
}
