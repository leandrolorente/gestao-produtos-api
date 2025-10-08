using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Driver;

namespace GestaoProdutos.Infrastructure.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;

    protected BaseRepository(MongoDbContext context)
    {
        _collection = GetCollection(context);
    }

    protected virtual IMongoCollection<T> GetCollection(MongoDbContext context)
    {
        if (typeof(T) == typeof(Produto))
            return (IMongoCollection<T>)context.Produtos;
        if (typeof(T) == typeof(Cliente))
            return (IMongoCollection<T>)context.Clientes;
        if (typeof(T) == typeof(Usuario))
            return (IMongoCollection<T>)context.Usuarios;

        throw new NotSupportedException($"Tipo {typeof(T).Name} não suportado");
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(entity => entity.Id == id)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection
            .Find(_ => true)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetByFilterAsync(Func<T, bool> predicate)
    {
        var allItems = await GetAllAsync();
        return allItems.Where(predicate);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.DataAtualizacao = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(entity => entity.Id == id);
        return result.DeletedCount > 0;
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(entity => entity.Id == id);
        return count > 0;
    }
}

public class ProdutoRepository : BaseRepository<Produto>, IProdutoRepository
{
    public ProdutoRepository(MongoDbContext context) : base(context) { }

    public async Task<IEnumerable<Produto>> GetProdutosComEstoqueBaixoAsync()
    {
        return await _collection
            .Find(p => p.Ativo && p.EstoqueMinimo != null && p.Quantidade <= p.EstoqueMinimo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(string categoria)
    {
        return await _collection
            .Find(p => p.Ativo && p.Categoria == categoria)
            .ToListAsync();
    }

    public async Task<Produto?> GetProdutoPorSkuAsync(string sku)
    {
        return await _collection
            .Find(p => p.Sku == sku && p.Ativo)
            .FirstOrDefaultAsync();
    }

    public async Task<Produto?> GetProdutoPorBarcodeAsync(string barcode)
    {
        return await _collection
            .Find(p => p.Barcode == barcode && p.Ativo)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SkuJaExisteAsync(string sku, string? produtoId = null)
    {
        var filter = Builders<Produto>.Filter.And(
            Builders<Produto>.Filter.Eq(p => p.Sku, sku),
            Builders<Produto>.Filter.Eq(p => p.Ativo, true)
        );

        if (!string.IsNullOrEmpty(produtoId))
        {
            filter = Builders<Produto>.Filter.And(
                filter,
                Builders<Produto>.Filter.Ne(p => p.Id, produtoId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<bool> BarcodeJaExisteAsync(string barcode, string? produtoId = null)
    {
        var filter = Builders<Produto>.Filter.And(
            Builders<Produto>.Filter.Eq(p => p.Barcode, barcode),
            Builders<Produto>.Filter.Eq(p => p.Ativo, true)
        );

        if (!string.IsNullOrEmpty(produtoId))
        {
            filter = Builders<Produto>.Filter.And(
                filter,
                Builders<Produto>.Filter.Ne(p => p.Id, produtoId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }
}
