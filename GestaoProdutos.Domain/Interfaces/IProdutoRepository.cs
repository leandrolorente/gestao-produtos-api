using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutosComEstoqueBaixoAsync();
    Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(string categoria);
    Task<Produto?> GetProdutoPorSkuAsync(string sku);
    Task<Produto?> GetProdutoPorBarcodeAsync(string barcode);
    Task<bool> SkuJaExisteAsync(string sku, string? produtoId = null);
    Task<bool> BarcodeJaExisteAsync(string barcode, string? produtoId = null);
}
