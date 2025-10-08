using GestaoProdutos.Application.DTOs;

namespace GestaoProdutos.Application.Interfaces;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> GetAllProdutosAsync();
    Task<ProdutoDto?> GetProdutoByIdAsync(string id);
    Task<ProdutoDto?> GetProdutoBySkuAsync(string sku);
    Task<ProdutoDto?> GetProdutoByBarcodeAsync(string barcode);
    Task<IEnumerable<ProdutoDto>> GetProdutosComEstoqueBaixoAsync();
    Task<IEnumerable<ProdutoDto>> GetProdutosPorCategoriaAsync(string categoria);
    Task<ProdutoDto> CreateProdutoAsync(CreateProdutoDto dto);
    Task<ProdutoDto> UpdateProdutoAsync(string id, UpdateProdutoDto dto);
    Task<bool> DeleteProdutoAsync(string id);
    Task<bool> UpdateEstoqueAsync(string id, int novaQuantidade);
}
