using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GestaoProdutos.Infrastructure.Repositories;

/// <summary>
/// Repositório para contas a pagar
/// </summary>
public class ContaPagarRepository : IContaPagarRepository
{
    private readonly IMongoCollection<ContaPagar> _collection;

    public ContaPagarRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<ContaPagar>("contasPagar");
        CreateIndexesAsync().Wait();
    }

    public async Task<ContaPagar?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id && x.Ativo).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetAllAsync()
    {
        return await _collection.Find(x => x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetByStatusAsync(StatusContaPagar status)
    {
        return await _collection.Find(x => x.Status == status && x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetVencidasAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        return await _collection.Find(x => 
            x.DataVencimento.Date < hoje && 
            x.Status != StatusContaPagar.Paga && 
            x.Status != StatusContaPagar.Cancelada && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetByFornecedorAsync(string fornecedorId)
    {
        return await _collection.Find(x => x.FornecedorId == fornecedorId && x.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _collection.Find(x => 
            x.DataVencimento >= inicio && 
            x.DataVencimento <= fim && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetVencendoEmAsync(int dias)
    {
        var dataLimite = DateTime.UtcNow.Date.AddDays(dias);
        return await _collection.Find(x => 
            x.DataVencimento.Date <= dataLimite && 
            x.DataVencimento.Date >= DateTime.UtcNow.Date &&
            x.Status == StatusContaPagar.Pendente && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<IEnumerable<ContaPagar>> GetByCategoriaAsync(CategoriaConta categoria)
    {
        return await _collection.Find(x => 
            x.Categoria == categoria && 
            x.Ativo
        ).ToListAsync();
    }

    public async Task<ContaPagar> CreateAsync(ContaPagar conta)
    {
        conta.Numero = await GetProximoNumeroAsync();
        await _collection.InsertOneAsync(conta);
        return conta;
    }

    public async Task<ContaPagar> UpdateAsync(ContaPagar conta)
    {
        conta.DataAtualizacao = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == conta.Id, conta);
        return conta;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<ContaPagar>.Update
            .Set(x => x.Ativo, false)
            .Set(x => x.DataAtualizacao, DateTime.UtcNow);
        
        var result = await _collection.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<string> GetProximoNumeroAsync()
    {
        var ultimaConta = await _collection
            .Find(x => x.Numero.StartsWith("CP-"))
            .SortByDescending(x => x.Numero)
            .FirstOrDefaultAsync();

        if (ultimaConta == null)
        {
            return "CP-001";
        }

        var ultimoNumero = int.Parse(ultimaConta.Numero.Replace("CP-", ""));
        return $"CP-{(ultimoNumero + 1):000}";
    }

    public async Task<decimal> GetTotalPagarPorPeriodoAsync(DateTime inicio, DateTime fim)
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
                ["status"] = new BsonDocument("$ne", (int)StatusContaPagar.Cancelada)
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

    public async Task<decimal> GetTotalPagoPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                ["dataPagamento"] = new BsonDocument
                {
                    ["$gte"] = inicio,
                    ["$lte"] = fim
                },
                ["ativo"] = true,
                ["status"] = (int)StatusContaPagar.Paga
            }),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["total"] = new BsonDocument("$sum", "$valorPago")
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
            x.Status != StatusContaPagar.Paga && 
            x.Status != StatusContaPagar.Cancelada && 
            x.Ativo
        );
    }

    public async Task<IEnumerable<ContaPagar>> GetRecorrentesParaGerarAsync()
    {
        return await _collection.Find(x => 
            x.EhRecorrente && 
            x.Status == StatusContaPagar.Paga && 
            x.Ativo
        ).ToListAsync();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            var indexKeys = Builders<ContaPagar>.IndexKeys;
            
            // Índice por número (único)
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaPagar>(indexKeys.Ascending(x => x.Numero)));
            
            // Índice por fornecedor
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaPagar>(indexKeys.Ascending(x => x.FornecedorId)));
            
            // Índice por status
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaPagar>(indexKeys.Ascending(x => x.Status)));
            
            // Índice por data de vencimento
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaPagar>(indexKeys.Ascending(x => x.DataVencimento)));
            
            // Índice composto para consultas por período
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ContaPagar>(indexKeys
                    .Ascending(x => x.DataVencimento)
                    .Ascending(x => x.Status)
                    .Ascending(x => x.Ativo)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Aviso: Não foi possível criar índices para contas a pagar: {ex.Message}");
        }
    }
}
