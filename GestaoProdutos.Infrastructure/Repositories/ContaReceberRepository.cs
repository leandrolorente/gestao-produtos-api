using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GestaoProdutos.Infrastructure.Repositories;

/// <summary>
/// Repositório para contas a receber
/// </summary>
public class ContaReceberRepository : IContaReceberRepository
{
    private readonly IMongoCollection<ContaReceber> _collection;

    public ContaReceberRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<ContaReceber>("contasReceber");
        CreateIndexesAsync().Wait();
    }

    public async Task<ContaReceber?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id && x.Ativo).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetAllAsync()
    {
        return await _collection.Find(x => x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetByStatusAsync(StatusContaReceber status)
    {
        return await _collection.Find(x => x.Status == status && x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetVencidasAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        return await _collection.Find(x => 
            x.DataVencimento.Date < hoje && 
            x.Status != StatusContaReceber.Recebida && 
            x.Status != StatusContaReceber.Cancelada && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetByClienteAsync(string clienteId)
    {
        return await _collection.Find(x => x.ClienteId == clienteId && x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _collection.Find(x => 
            x.DataVencimento >= inicio && 
            x.DataVencimento <= fim && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetVencendoEmAsync(int dias)
    {
        var dataLimite = DateTime.UtcNow.Date.AddDays(dias);
        return await _collection.Find(x => 
            x.DataVencimento.Date <= dataLimite && 
            x.DataVencimento.Date >= DateTime.UtcNow.Date &&
            x.Status == StatusContaReceber.Pendente && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<ContaReceber> CreateAsync(ContaReceber conta)
    {
        conta.Numero = await GetProximoNumeroAsync();
        await _collection.InsertOneAsync(conta);
        return conta;
    }

    public async Task<ContaReceber> UpdateAsync(ContaReceber conta)
    {
        conta.DataAtualizacao = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == conta.Id, conta);
        return conta;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<ContaReceber>.Update
            .Set(x => x.Ativo, false)
            .Set(x => x.DataAtualizacao, DateTime.UtcNow);
        
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<string> GetProximoNumeroAsync()
    {
        var ultimaConta = await _collection
            .Find(x => x.Numero.StartsWith("CR-"))
            .SortByDescending(x => x.Numero)
            .FirstOrDefaultAsync();

        if (ultimaConta == null)
        {
            return "CR-001";
        }

        var ultimoNumero = int.Parse(ultimaConta.Numero.Replace("CR-", ""));
        return $"CR-{(ultimoNumero + 1):000}";
    }

    public async Task<decimal> GetTotalReceberPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                ["dataVencimento"] = new BsonDocument
                {
                    ["$gte"] = inicio,
                    ["$lte"] = fim
                },
                ["ativo"] = true,
                ["status"] = new BsonDocument("$ne", (int)StatusContaReceber.Cancelada)
            }),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["total"] = new BsonDocument("$sum", "$valorOriginal")
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        return result?["total"]?.AsDecimal ?? 0;
    }

    public async Task<decimal> GetTotalRecebidoPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                ["dataRecebimento"] = new BsonDocument
                {
                    ["$gte"] = inicio,
                    ["$lte"] = fim
                },
                ["ativo"] = true,
                ["status"] = (int)StatusContaReceber.Recebida
            }),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["total"] = new BsonDocument("$sum", "$valorRecebido")
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        return result?["total"]?.AsDecimal ?? 0;
    }

    public async Task<int> GetQuantidadeVencidasAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        return (int)await _collection.CountDocumentsAsync(x => 
            x.DataVencimento.Date < hoje && 
            x.Status != StatusContaReceber.Recebida && 
            x.Status != StatusContaReceber.Cancelada && 
            x.Ativo
        );
    }

    public async Task<IEnumerable<ContaReceber>> GetRecorrentesParaGerarAsync()
    {
        return await _collection.Find(x => 
            x.EhRecorrente && 
            x.Status == StatusContaReceber.Recebida && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaReceber>> GetByVendedorAsync(string vendedorId)
    {
        return await _collection.Find(x => x.VendedorId == vendedorId && x.Ativo).ToListAsync();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            var indexKeys = Builders<ContaReceber>.IndexKeys;
            
            // Índice por número (único)
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys.Ascending(x => x.Numero)));
            
            // Índice por cliente
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys.Ascending(x => x.ClienteId)));
            
            // Índice por status
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys.Ascending(x => x.Status)));
            
            // Índice por data de vencimento
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys.Ascending(x => x.DataVencimento)));
            
            // Índice por vendedor
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys.Ascending(x => x.VendedorId)));
            
            // Índice composto para consultas por período
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaReceber>(indexKeys
                    .Ascending(x => x.DataVencimento)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.Ativo)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Aviso: Não foi possível criar índices para contas a receber: {ex.Message}");
        }
    }
}
