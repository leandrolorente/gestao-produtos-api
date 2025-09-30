using MongoDB.Driver;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Bson;
using System.Globalization;

namespace GestaoProdutos.Infrastructure.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly IMongoCollection<Venda> _collection;

    public VendaRepository(MongoDbContext context)
    {
        _collection = context.Vendas;
        CreateIndexesAsync().Wait();
    }

    public async Task<Venda?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _collection.Find(v => v.Id == id && v.Ativo).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Venda>> GetAllAsync()
    {
        return await _collection.Find(v => v.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetByFilterAsync(Func<Venda, bool> predicate)
    {
        var vendas = await _collection.Find(v => v.Ativo).ToListAsync();
        return vendas.Where(predicate);
    }

    public async Task<Venda> CreateAsync(Venda entity)
    {
        entity.Id = ObjectId.GenerateNewId().ToString();
        entity.DataCriacao = DateTime.UtcNow;
        entity.DataAtualizacao = DateTime.UtcNow;
        entity.Ativo = true;

        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<Venda> UpdateAsync(Venda entity)
    {
        entity.DataAtualizacao = DateTime.UtcNow;

        await _collection.ReplaceOneAsync(
            v => v.Id == entity.Id && v.Ativo,
            entity
        );

        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var update = Builders<Venda>.Update
            .Set(v => v.Ativo, false)
            .Set(v => v.DataAtualizacao, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(
            v => v.Id == id && v.Ativo,
            update
        );

        return result.MatchedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        return await _collection.CountDocumentsAsync(v => v.Id == id && v.Ativo) > 0;
    }

    public async Task<Venda?> GetVendaPorNumeroAsync(string numero)
    {
        return await _collection.Find(v => v.Numero == numero && v.Ativo).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasPorClienteAsync(string clienteId)
    {
        return await _collection.Find(v => v.ClienteId == clienteId && v.Ativo)
            .SortByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasPorVendedorAsync(string vendedorId)
    {
        return await _collection.Find(v => v.VendedorId == vendedorId && v.Ativo)
            .SortByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasPorStatusAsync(StatusVenda status)
    {
        return await _collection.Find(v => v.Status == status && v.Ativo)
            .SortByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var filter = Builders<Venda>.Filter.And(
            Builders<Venda>.Filter.Eq(v => v.Ativo, true),
            Builders<Venda>.Filter.Gte(v => v.DataVenda, dataInicio),
            Builders<Venda>.Filter.Lte(v => v.DataVenda, dataFim)
        );

        return await _collection.Find(filter)
            .SortByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasVencidasAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var filter = Builders<Venda>.Filter.And(
            Builders<Venda>.Filter.Eq(v => v.Ativo, true),
            Builders<Venda>.Filter.Eq(v => v.Status, StatusVenda.Pendente),
            Builders<Venda>.Filter.Ne(v => v.DataVencimento, null),
            Builders<Venda>.Filter.Lt(v => v.DataVencimento, hoje)
        );

        return await _collection.Find(filter)
            .SortBy(v => v.DataVencimento)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venda>> GetVendasHojeAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var amanha = hoje.AddDays(1);

        var filter = Builders<Venda>.Filter.And(
            Builders<Venda>.Filter.Eq(v => v.Ativo, true),
            Builders<Venda>.Filter.Gte(v => v.DataVenda, hoje),
            Builders<Venda>.Filter.Lt(v => v.DataVenda, amanha)
        );

        return await _collection.Find(filter)
            .SortByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    public async Task<decimal> GetFaturamentoPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var filter = Builders<Venda>.Filter.And(
            Builders<Venda>.Filter.Eq(v => v.Ativo, true),
            Builders<Venda>.Filter.In(v => v.Status, new[] { StatusVenda.Confirmada, StatusVenda.Finalizada }),
            Builders<Venda>.Filter.Gte(v => v.DataVenda, dataInicio),
            Builders<Venda>.Filter.Lte(v => v.DataVenda, dataFim)
        );

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                {"Ativo", true},
                {"Status", new BsonDocument("$in", new BsonArray { (int)StatusVenda.Confirmada, (int)StatusVenda.Finalizada })}
            }),
            new BsonDocument("$group", new BsonDocument
            {
                {"_id", BsonNull.Value},
                {"total", new BsonDocument("$sum", "$Total")}
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        return result != null ? ConvertToDecimal(result["total"]) : 0;
    }

    public async Task<string> GetProximoNumeroVendaAsync()
    {
        var ultimaVenda = await _collection.Find(v => v.Ativo)
            .SortByDescending(v => v.Numero)
            .FirstOrDefaultAsync();

        if (ultimaVenda == null || string.IsNullOrEmpty(ultimaVenda.Numero))
        {
            return "VND-001";
        }

        // Extrai o número sequencial do formato VND-XXX
        var numeroStr = ultimaVenda.Numero.Replace("VND-", "");
        if (int.TryParse(numeroStr, out int numero))
        {
            return $"VND-{(numero + 1):D3}";
        }

        return "VND-001";
    }

    public async Task<bool> NumeroVendaJaExisteAsync(string numero, string? vendaId = null)
    {
        var filter = Builders<Venda>.Filter.And(
            Builders<Venda>.Filter.Eq(v => v.Numero, numero),
            Builders<Venda>.Filter.Eq(v => v.Ativo, true)
        );

        if (!string.IsNullOrEmpty(vendaId))
        {
            filter = Builders<Venda>.Filter.And(
                filter,
                Builders<Venda>.Filter.Ne(v => v.Id, vendaId)
            );
        }

        return await _collection.CountDocumentsAsync(filter) > 0;
    }

    public async Task<IEnumerable<TopClienteResult>> GetTopClientesAsync(int quantidade = 10)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                {"Ativo", true},
                {"Status", new BsonDocument("$in", new BsonArray { (int)StatusVenda.Confirmada, (int)StatusVenda.Finalizada })}
            }),
            new BsonDocument("$group", new BsonDocument
            {
                {"_id", new BsonDocument
                {
                    {"clienteId", "$ClienteId"},
                    {"clienteNome", "$ClienteNome"}
                }},
                {"totalCompras", new BsonDocument("$sum", 1)},
                {"valorTotal", new BsonDocument("$sum", "$Total")}
            }),
            new BsonDocument("$sort", new BsonDocument("valorTotal", -1)),
            new BsonDocument("$limit", quantidade),
            new BsonDocument("$project", new BsonDocument
            {
                {"_id", 0},
                {"clienteNome", "$_id.clienteNome"},
                {"totalCompras", 1},
                {"valorTotal", 1}
            })
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return results.Select(doc => new TopClienteResult
        {
            ClienteNome = doc["clienteNome"].AsString,
            TotalCompras = doc["totalCompras"].AsInt32,
            ValorTotal = ConvertToDecimal(doc["valorTotal"])
        });
    }

    public async Task<IEnumerable<VendasPorMesResult>> GetVendasPorMesAsync(int meses = 12)
    {
        var dataInicio = DateTime.UtcNow.AddMonths(-meses).Date;
        
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                {"Ativo", true},
                {"DataVenda", new BsonDocument("$gte", dataInicio)}
            }),
            new BsonDocument("$group", new BsonDocument
            {
                {"_id", new BsonDocument
                {
                    {"ano", new BsonDocument("$year", "$DataVenda")},
                    {"mes", new BsonDocument("$month", "$DataVenda")}
                }},
                {"vendas", new BsonDocument("$sum", 1)},
                {"faturamento", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                {
                    new BsonDocument("$in", new BsonArray { "$Status", new BsonArray { (int)StatusVenda.Confirmada, (int)StatusVenda.Finalizada } }),
                    "$Total",
                    0
                }))}
            }),
            new BsonDocument("$sort", new BsonDocument
            {
                {"_id.ano", 1},
                {"_id.mes", 1}
            }),
            new BsonDocument("$project", new BsonDocument
            {
                {"_id", 0},
                {"mes", new BsonDocument("$switch", new BsonDocument
                {
                    {"branches", new BsonArray
                    {
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 1})}, {"then", "Jan"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 2})}, {"then", "Fev"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 3})}, {"then", "Mar"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 4})}, {"then", "Abr"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 5})}, {"then", "Mai"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 6})}, {"then", "Jun"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 7})}, {"then", "Jul"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 8})}, {"then", "Ago"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 9})}, {"then", "Set"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 10})}, {"then", "Out"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 11})}, {"then", "Nov"} },
                        new BsonDocument { {"case", new BsonDocument("$eq", new BsonArray {"$_id.mes", 12})}, {"then", "Dez"} }
                    }},
                    {"default", "???"}
                })},
                {"vendas", 1},
                {"faturamento", 1}
            })
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        return results.Select(doc => new VendasPorMesResult
        {
            Mes = doc["mes"].AsString,
            Vendas = doc["vendas"].AsInt32,
            Faturamento = ConvertToDecimal(doc["faturamento"])
        });
    }

    private static decimal ConvertToDecimal(BsonValue bsonValue)
    {
        return bsonValue.BsonType switch
        {
            BsonType.Decimal128 => bsonValue.AsDecimal,
            BsonType.Double => (decimal)bsonValue.AsDouble,
            BsonType.Int32 => (decimal)bsonValue.AsInt32,
            BsonType.Int64 => (decimal)bsonValue.AsInt64,
            _ => 0m
        };
    }

    private async Task CreateIndexesAsync()
    {
        var indexKeys = Builders<Venda>.IndexKeys;

        var indexes = new[]
        {
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.Numero)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.ClienteId)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.VendedorId)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.Status)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.DataVenda)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.DataVencimento)),
            new CreateIndexModel<Venda>(indexKeys.Ascending(v => v.Ativo).Ascending(v => v.Status).Descending(v => v.DataVenda))
        };

        await _collection.Indexes.CreateManyAsync(indexes);
    }
}