using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Driver;

namespace GestaoProdutos.Infrastructure.Repositories;

/// <summary>
/// Repositório para operações com fornecedores no MongoDB
/// </summary>
public class FornecedorRepository : IFornecedorRepository
{
    private readonly IMongoCollection<Fornecedor> _collection;

    public FornecedorRepository(MongoDbContext context)
    {
        _collection = context.Fornecedores;
        
        // Criar índices para otimização
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            // Índice único para CNPJ/CPF
            var cnpjCpfIndex = Builders<Fornecedor>.IndexKeys.Ascending(f => f.CnpjCpf.Valor);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                cnpjCpfIndex, 
                new CreateIndexOptions { Unique = true, Name = "idx_cnpj_cpf" }
            ));

            // Índice para email
            var emailIndex = Builders<Fornecedor>.IndexKeys.Ascending(f => f.Email.Valor);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                emailIndex,
                new CreateIndexOptions { Name = "idx_email" }
            ));

            // Índice para razão social (busca de texto)
            var razaoSocialIndex = Builders<Fornecedor>.IndexKeys.Text(f => f.RazaoSocial);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                razaoSocialIndex,
                new CreateIndexOptions { Name = "idx_razao_social_text" }
            ));

            // Índice composto para status e tipo
            var statusTipoIndex = Builders<Fornecedor>.IndexKeys
                .Ascending(f => f.Status)
                .Ascending(f => f.Tipo)
                .Ascending(f => f.Ativo);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                statusTipoIndex,
                new CreateIndexOptions { Name = "idx_status_tipo_ativo" }
            ));

            // Índice para última compra
            var ultimaCompraIndex = Builders<Fornecedor>.IndexKeys.Descending(f => f.UltimaCompra);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                ultimaCompraIndex,
                new CreateIndexOptions { Name = "idx_ultima_compra" }
            ));

            // Índice para produtos (array)
            var produtosIndex = Builders<Fornecedor>.IndexKeys.Ascending(f => f.ProdutoIds);
            _collection.Indexes.CreateOne(new CreateIndexModel<Fornecedor>(
                produtosIndex,
                new CreateIndexOptions { Name = "idx_produtos" }
            ));
        }
        catch (Exception ex)
        {
            // Log de erro mas não quebra a aplicação
            Console.WriteLine($"Aviso: Não foi possível criar índices para fornecedores: {ex.Message}");
        }
    }

    public async Task<Fornecedor?> GetByIdAsync(string id)
    {
        return await _collection
            .Find(f => f.Id == id && f.Ativo)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
    {
        return await _collection
            .Find(f => f.Ativo)
            .SortBy(f => f.RazaoSocial)
            .ToListAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetByFilterAsync(Func<Fornecedor, bool> predicate)
    {
        var allFornecedores = await GetAllAsync();
        return allFornecedores.Where(predicate);
    }

    public async Task<Fornecedor> CreateAsync(Fornecedor entity)
    {
        entity.DataCriacao = DateTime.UtcNow;
        entity.DataAtualizacao = DateTime.UtcNow;
        entity.Ativo = true;

        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<Fornecedor> UpdateAsync(Fornecedor entity)
    {
        entity.DataAtualizacao = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(
            f => f.Id == entity.Id,
            entity
        );

        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        // Soft delete - marca como inativo
        var update = Builders<Fornecedor>.Update
            .Set(f => f.Ativo, false)
            .Set(f => f.DataAtualizacao, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(
            f => f.Id == id,
            update
        );

        return result.ModifiedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection
            .Find(f => f.Id == id && f.Ativo)
            .AnyAsync();
    }

    // Métodos específicos para fornecedores

    public async Task<Fornecedor?> GetFornecedorPorCnpjCpfAsync(string cnpjCpf)
    {
        return await _collection
            .Find(f => f.CnpjCpf.Valor == cnpjCpf && f.Ativo)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetFornecedoresAtivosPorTipoAsync(TipoFornecedor tipo)
    {
        return await _collection
            .Find(f => f.Tipo == tipo && f.Status == StatusFornecedor.Ativo && f.Ativo)
            .SortBy(f => f.RazaoSocial)
            .ToListAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetFornecedoresComCompraRecenteAsync(int dias = 90)
    {
        var dataLimite = DateTime.UtcNow.AddDays(-dias);
        
        return await _collection
            .Find(f => f.UltimaCompra >= dataLimite && f.Ativo)
            .SortByDescending(f => f.UltimaCompra)
            .ToListAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetFornecedoresPorStatusAsync(StatusFornecedor status)
    {
        return await _collection
            .Find(f => f.Status == status && f.Ativo)
            .SortBy(f => f.RazaoSocial)
            .ToListAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetFornecedoresPorProdutoAsync(string produtoId)
    {
        return await _collection
            .Find(f => f.ProdutoIds.Contains(produtoId) && f.Ativo)
            .SortBy(f => f.RazaoSocial)
            .ToListAsync();
    }

    public async Task<bool> CnpjCpfJaExisteAsync(string cnpjCpf, string? fornecedorId)
    {
        var filter = Builders<Fornecedor>.Filter.And(
            Builders<Fornecedor>.Filter.Eq(f => f.CnpjCpf.Valor, cnpjCpf),
            Builders<Fornecedor>.Filter.Eq(f => f.Ativo, true)
        );

        if (!string.IsNullOrEmpty(fornecedorId))
        {
            filter = Builders<Fornecedor>.Filter.And(
                filter,
                Builders<Fornecedor>.Filter.Ne(f => f.Id, fornecedorId)
            );
        }

        return await _collection.Find(filter).AnyAsync();
    }

    public async Task<IEnumerable<Fornecedor>> GetFornecedoresFrequentesAsync()
    {
        var dataLimite = DateTime.UtcNow.AddDays(-90);
        
        return await _collection
            .Find(f => f.UltimaCompra >= dataLimite && f.QuantidadeCompras >= 3 && f.Ativo)
            .SortByDescending(f => f.TotalComprado)
            .ToListAsync();
    }

    public async Task<IEnumerable<Fornecedor>> BuscarFornecedoresAsync(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            return await GetAllAsync();
        }

        var termoLimpo = termo.Trim().ToLowerInvariant();

        // Busca por texto em múltiplos campos
        var filter = Builders<Fornecedor>.Filter.And(
            Builders<Fornecedor>.Filter.Eq(f => f.Ativo, true),
            Builders<Fornecedor>.Filter.Or(
                Builders<Fornecedor>.Filter.Regex(f => f.RazaoSocial, new MongoDB.Bson.BsonRegularExpression(termoLimpo, "i")),
                Builders<Fornecedor>.Filter.Regex(f => f.NomeFantasia, new MongoDB.Bson.BsonRegularExpression(termoLimpo, "i")),
                Builders<Fornecedor>.Filter.Regex(f => f.CnpjCpf.Valor, new MongoDB.Bson.BsonRegularExpression(termoLimpo, "i")),
                Builders<Fornecedor>.Filter.Regex(f => f.Email.Valor, new MongoDB.Bson.BsonRegularExpression(termoLimpo, "i")),
                Builders<Fornecedor>.Filter.Regex(f => f.ContatoPrincipal, new MongoDB.Bson.BsonRegularExpression(termoLimpo, "i"))
            )
        );

        return await _collection
            .Find(filter)
            .SortBy(f => f.RazaoSocial)
            .ToListAsync();
    }
}