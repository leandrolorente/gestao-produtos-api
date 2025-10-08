using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Application.Interfaces;

/// <summary>
/// Interface para serviços de contas a receber
/// </summary>
public interface IContaReceberService
{
    Task<IEnumerable<ContaReceberDto>> GetAllContasReceberAsync();
    Task<ContaReceberDto?> GetContaReceberByIdAsync(string id);
    Task<IEnumerable<ContaReceberDto>> GetContasReceberByStatusAsync(StatusContaReceber status);
    Task<IEnumerable<ContaReceberDto>> GetContasReceberVencidasAsync();
    Task<IEnumerable<ContaReceberDto>> GetContasReceberByClienteAsync(string clienteId);
    Task<IEnumerable<ContaReceberDto>> GetContasReceberByPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IEnumerable<ContaReceberDto>> GetContasReceberVencendoEmAsync(int dias);
    Task<ContaReceberDto> CreateContaReceberAsync(CreateContaReceberDto dto);
    Task<ContaReceberDto> UpdateContaReceberAsync(string id, UpdateContaReceberDto dto);
    Task<bool> DeleteContaReceberAsync(string id);
    Task<ContaReceberDto> ReceberContaAsync(string id, ReceberContaDto dto);
    Task<bool> CancelarContaAsync(string id);
    Task<decimal> GetTotalReceberPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<decimal> GetTotalRecebidoPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<int> GetQuantidadeContasVencidasAsync();
    Task<IEnumerable<ContaReceberDto>> GetContasReceberByVendedorAsync(string vendedorId);
    Task AtualizarStatusContasAsync();
    Task ProcessarContasRecorrentesAsync();
}
