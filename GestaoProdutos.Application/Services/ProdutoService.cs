using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProdutoDto>> GetAllProdutosAsync()
    {
        var produtos = await _unitOfWork.Produtos.GetAllAsync();
        return produtos.Where(p => p.Ativo).Select(MapToDto);
    }

    public async Task<ProdutoDto?> GetProdutoByIdAsync(string id)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        return produto?.Ativo == true ? MapToDto(produto) : null;
    }

    public async Task<ProdutoDto?> GetProdutoBySkuAsync(string sku)
    {
        var produto = await _unitOfWork.Produtos.GetProdutoPorSkuAsync(sku);
        return produto?.Ativo == true ? MapToDto(produto) : null;
    }

    public async Task<ProdutoDto?> GetProdutoByBarcodeAsync(string barcode)
    {
        var produto = await _unitOfWork.Produtos.GetProdutoPorBarcodeAsync(barcode);
        return produto?.Ativo == true ? MapToDto(produto) : null;
    }

    public async Task<IEnumerable<ProdutoDto>> GetProdutosComEstoqueBaixoAsync()
    {
        var produtos = await _unitOfWork.Produtos.GetProdutosComEstoqueBaixoAsync();
        return produtos.Where(p => p.Ativo).Select(MapToDto);
    }

    public async Task<IEnumerable<ProdutoDto>> GetProdutosPorCategoriaAsync(string categoria)
    {
        var produtos = await _unitOfWork.Produtos.GetProdutosPorCategoriaAsync(categoria);
        return produtos.Where(p => p.Ativo).Select(MapToDto);
    }

    public async Task<ProdutoDto> CreateProdutoAsync(CreateProdutoDto dto)
    {
        // Validar se SKU já existe
        if (await _unitOfWork.Produtos.SkuJaExisteAsync(dto.Sku))
        {
            throw new InvalidOperationException("SKU já existe");
        }

        // Validar se Barcode já existe (se foi fornecido)
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            if (await _unitOfWork.Produtos.BarcodeJaExisteAsync(dto.Barcode))
            {
                throw new InvalidOperationException("Código de barras já existe");
            }
        }

        var produto = new Produto
        {
            Nome = dto.Name,
            Sku = dto.Sku,
            Barcode = dto.Barcode,
            Quantidade = dto.Quantity,
            Preco = dto.Price,
            Categoria = dto.Categoria,
            Descricao = dto.Descricao,
            PrecoCompra = dto.PrecoCompra,
            EstoqueMinimo = dto.EstoqueMinimo
        };

        await _unitOfWork.Produtos.CreateAsync(produto);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(produto);
    }

    public async Task<ProdutoDto> UpdateProdutoAsync(string id, UpdateProdutoDto dto)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || !produto.Ativo)
        {
            throw new ArgumentException("Produto não encontrado");
        }

        // Validar se Barcode já existe (se foi fornecido e é diferente do atual)
        if (!string.IsNullOrWhiteSpace(dto.Barcode) && dto.Barcode != produto.Barcode)
        {
            if (await _unitOfWork.Produtos.BarcodeJaExisteAsync(dto.Barcode, id))
            {
                throw new InvalidOperationException("Código de barras já existe");
            }
        }

        // Atualizar propriedades
        produto.Nome = dto.Name;
        produto.Barcode = dto.Barcode;
        produto.Quantidade = dto.Quantity;
        produto.Preco = dto.Price;
        produto.Categoria = dto.Categoria;
        produto.Descricao = dto.Descricao;
        produto.PrecoCompra = dto.PrecoCompra;
        produto.EstoqueMinimo = dto.EstoqueMinimo;
        produto.DataAtualizacao = DateTime.UtcNow;
        produto.UltimaAtualizacao = DateTime.UtcNow;

        try
        {
            await _unitOfWork.Produtos.UpdateAsync(produto);
            await _unitOfWork.SaveChangesAsync();
            
            // Recarregar o produto para garantir que foi salvo
            var produtoAtualizado = await _unitOfWork.Produtos.GetByIdAsync(id);
            if (produtoAtualizado == null)
            {
                throw new InvalidOperationException("Produto não foi encontrado após atualização");
            }
            
            return MapToDto(produtoAtualizado);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao atualizar produto: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteProdutoAsync(string id)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null) return false;

        produto.Ativo = false;
        produto.DataAtualizacao = DateTime.UtcNow;

        await _unitOfWork.Produtos.UpdateAsync(produto);
        return await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UpdateEstoqueAsync(string id, int novaQuantidade)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || !produto.Ativo) return false;

        produto.AtualizarEstoque(novaQuantidade);
        await _unitOfWork.Produtos.UpdateAsync(produto);
        return await _unitOfWork.SaveChangesAsync();
    }

    private static ProdutoDto MapToDto(Produto produto)
    {
        return new ProdutoDto
        {
            Id = produto.Id,
            Name = produto.Nome,
            Sku = produto.Sku,
            Barcode = produto.Barcode,
            Quantity = produto.Quantidade,
            Price = produto.Preco,
            LastUpdated = produto.UltimaAtualizacao,
            Categoria = produto.Categoria,
            EstoqueBaixo = produto.EstaComEstoqueBaixo()
        };
    }
}