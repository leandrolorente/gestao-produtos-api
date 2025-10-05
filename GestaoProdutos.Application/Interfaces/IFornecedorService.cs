using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

/// <summary>
/// Interface para serviço de fornecedores
/// </summary>
public interface IFornecedorService
{
    Task<IEnumerable<FornecedorDto>> GetAllFornecedoresAsync();
    Task<IEnumerable<FornecedorListDto>> GetAllFornecedoresListAsync();
    Task<FornecedorDto?> GetFornecedorByIdAsync(string id);
    Task<FornecedorDto?> GetFornecedorByCnpjCpfAsync(string cnpjCpf);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresAtivosPorTipoAsync(Domain.Enums.TipoFornecedor tipo);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresComCompraRecenteAsync(int dias = 90);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresPorStatusAsync(Domain.Enums.StatusFornecedor status);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresPorProdutoAsync(string produtoId);
    Task<IEnumerable<FornecedorDto>> GetFornecedoresFrequentesAsync();
    Task<IEnumerable<FornecedorDto>> BuscarFornecedoresAsync(string termo);
    Task<FornecedorDto> CreateFornecedorAsync(CreateFornecedorDto dto);
    Task<FornecedorDto> UpdateFornecedorAsync(string id, UpdateFornecedorDto dto);
    Task<bool> DeleteFornecedorAsync(string id);
    Task<bool> BloquearFornecedorAsync(string id, string? motivo);
    Task<bool> DesbloquearFornecedorAsync(string id);
    Task<bool> InativarFornecedorAsync(string id);
    Task<bool> AtivarFornecedorAsync(string id);
    Task<bool> AdicionarProdutoAsync(string fornecedorId, string produtoId);
    Task<bool> RemoverProdutoAsync(string fornecedorId, string produtoId);
    Task<bool> RegistrarCompraAsync(string fornecedorId, decimal valor);
    Task<bool> AtualizarCondicoesComerciais(string fornecedorId, int prazoPagamento, decimal limiteCredito, string? condicoesPagamento);
    Task<bool> AtualizarDadosBancarios(string fornecedorId, string? banco, string? agencia, string? conta, string? pix);
}