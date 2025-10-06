using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

public interface IFornecedorRepository : IRepository<Fornecedor>
{
    Task<Fornecedor?> GetFornecedorPorCnpjCpfAsync(string cnpjCpf);
    Task<IEnumerable<Fornecedor>> GetFornecedoresAtivosPorTipoAsync(TipoFornecedor tipo);
    Task<IEnumerable<Fornecedor>> GetFornecedoresComCompraRecenteAsync(int dias = 90);
    Task<IEnumerable<Fornecedor>> GetFornecedoresPorStatusAsync(StatusFornecedor status);
    Task<IEnumerable<Fornecedor>> GetFornecedoresPorProdutoAsync(string produtoId);
    Task<bool> CnpjCpfJaExisteAsync(string cnpjCpf, string? fornecedorId);
    Task<IEnumerable<Fornecedor>> GetFornecedoresFrequentesAsync();
    Task<IEnumerable<Fornecedor>> BuscarFornecedoresAsync(string termo);
}