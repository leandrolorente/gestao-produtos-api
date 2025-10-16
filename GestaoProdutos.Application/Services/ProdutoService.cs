using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(
        IUnitOfWork unitOfWork, 
        ICacheService cache,
        ILogger<ProdutoService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoDto>> GetAllProdutosAsync()
    {
        const string cacheKey = "gp:produtos:list:all";
        
        // Tentar buscar do cache
        var cachedProdutos = await _cache.GetAsync<IEnumerable<ProdutoDto>>(cacheKey);
        if (cachedProdutos != null)
        {
            _logger.LogDebug("Lista de produtos recuperada do cache");
            return cachedProdutos;
        }

        // Buscar do banco e cachear
        var produtos = await _unitOfWork.Produtos.GetAllAsync();
        var produtosDto = produtos.Where(p => p.Ativo).Select(MapToDto).ToList();
        
        await _cache.SetAsync(cacheKey, produtosDto, TimeSpan.FromMinutes(10));
        _logger.LogDebug("Lista de produtos armazenada no cache por 10 minutos");
        
        return produtosDto;
    }

    public async Task<ProdutoDto?> GetProdutoByIdAsync(string id)
    {
        var cacheKey = $"gp:produto:{id}";
        
        // Tentar buscar do cache
        var cachedProduto = await _cache.GetAsync<ProdutoDto>(cacheKey);
        if (cachedProduto != null)
        {
            _logger.LogDebug("Produto {Id} recuperado do cache", id);
            return cachedProduto;
        }

        // Buscar do banco e cachear
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto?.Ativo == true)
        {
            var produtoDto = MapToDto(produto);
            await _cache.SetAsync(cacheKey, produtoDto, TimeSpan.FromMinutes(30));
            _logger.LogDebug("Produto {Id} armazenado no cache por 30 minutos", id);
            return produtoDto;
        }
        
        return null;
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

        // Invalidar caches relacionados
        await InvalidateProdutosCacheAsync();
        _logger.LogDebug("Cache de produtos invalidado após criação do produto {Id}", produto.Id);

        return MapToDto(produto);
    }

    public async Task<ProdutoDto> UpdateProdutoAsync(string id, UpdateProdutoDto dto)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || !produto.Ativo)
        {
            throw new ArgumentException("Produto não encontrado");
        }

        // Validar se SKU já existe (se foi alterado)
        if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != produto.Sku)
        {
            if (await _unitOfWork.Produtos.SkuJaExisteAsync(dto.Sku, id))
            {
                throw new InvalidOperationException("SKU já existe");
            }
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
        produto.Sku = dto.Sku; // SKU agora editável
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
            
            // Invalidar caches relacionados
            await InvalidateProdutosCacheAsync();
            await _cache.RemoveAsync($"gp:produto:{id}");
            _logger.LogDebug("Cache de produtos invalidado após atualização do produto {Id}", id);
            
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
        var result = await _unitOfWork.SaveChangesAsync();
        
        if (result)
        {
            // Invalidar caches relacionados
            await InvalidateProdutosCacheAsync();
            await _cache.RemoveAsync($"gp:produto:{id}");
            _logger.LogDebug("Cache de produtos invalidado após exclusão do produto {Id}", id);
        }
        
        return result;
    }

    public async Task<bool> UpdateEstoqueAsync(string id, int novaQuantidade)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || !produto.Ativo) return false;

        produto.AtualizarEstoque(novaQuantidade);
        await _unitOfWork.Produtos.UpdateAsync(produto);
        var result = await _unitOfWork.SaveChangesAsync();
        
        if (result)
        {
            // Invalidar caches relacionados
            await InvalidateProdutosCacheAsync();
            await _cache.RemoveAsync($"gp:produto:{id}");
            _logger.LogDebug("Cache de produtos invalidado após atualização de estoque do produto {Id}", id);
        }
        
        return result;
    }

    /// <summary>
    /// Invalida todos os caches relacionados a produtos
    /// </summary>
    private async Task InvalidateProdutosCacheAsync()
    {
        await _cache.RemovePatternAsync("gp:produto*");
        await _cache.RemovePatternAsync("gp:dashboard*");
        await _cache.RemovePatternAsync("gp:search:produtos*");
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
