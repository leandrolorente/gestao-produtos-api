using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GestaoProdutos.Infrastructure.Repositories;

public class EnderecoRepository : IEnderecoRepository
{
    private readonly IMongoCollection<EnderecoEntity> _enderecos;

    public EnderecoRepository(MongoDbContext context)
    {
        _enderecos = context.Enderecos;
    }

    public async Task<IEnumerable<EnderecoEntity>> GetAllAsync()
    {
        return await _enderecos.Find(e => e.Ativo).ToListAsync();
    }

    public async Task<EnderecoEntity?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _enderecos.Find(e => e.Id == id && e.Ativo).FirstOrDefaultAsync();
    }

    public async Task<EnderecoEntity?> GetByCepAsync(string cep)
    {
        return await _enderecos.Find(e => e.Cep == cep && e.Ativo).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<EnderecoEntity>> GetByCidadeAsync(string cidade)
    {
        return await _enderecos.Find(e => e.Localidade.ToLower() == cidade.ToLower() && e.Ativo).ToListAsync();
    }

    public async Task<IEnumerable<EnderecoEntity>> GetByEstadoAsync(string estado)
    {
        return await _enderecos.Find(e => e.Estado.ToLower() == estado.ToLower() && e.Ativo).ToListAsync();
    }

    public async Task<EnderecoEntity> CreateAsync(EnderecoEntity endereco)
    {
        endereco.DataCriacao = DateTime.UtcNow;
        endereco.DataAtualizacao = DateTime.UtcNow;
        endereco.Ativo = true;

        await _enderecos.InsertOneAsync(endereco);
        return endereco;
    }

    public async Task UpdateAsync(string id, EnderecoEntity endereco)
    {
        if (!ObjectId.TryParse(id, out _))
            return;

        endereco.Id = id;
        endereco.DataAtualizacao = DateTime.UtcNow;

        await _enderecos.ReplaceOneAsync(e => e.Id == id, endereco);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var updateDefinition = Builders<EnderecoEntity>.Update
            .Set(e => e.Ativo, false)
            .Set(e => e.DataAtualizacao, DateTime.UtcNow);

        var result = await _enderecos.UpdateOneAsync(e => e.Id == id, updateDefinition);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        return await _enderecos.CountDocumentsAsync(e => e.Id == id && e.Ativo) > 0;
    }

    public async Task<IEnumerable<EnderecoEntity>> SearchAsync(string termo)
    {
        var filter = Builders<EnderecoEntity>.Filter.And(
            Builders<EnderecoEntity>.Filter.Eq(e => e.Ativo, true),
            Builders<EnderecoEntity>.Filter.Or(
                Builders<EnderecoEntity>.Filter.Regex(e => e.Logradouro, new BsonRegularExpression(termo, "i")),
                Builders<EnderecoEntity>.Filter.Regex(e => e.Bairro, new BsonRegularExpression(termo, "i")),
                Builders<EnderecoEntity>.Filter.Regex(e => e.Localidade, new BsonRegularExpression(termo, "i")),
                Builders<EnderecoEntity>.Filter.Regex(e => e.Estado, new BsonRegularExpression(termo, "i")),
                Builders<EnderecoEntity>.Filter.Regex(e => e.Cep, new BsonRegularExpression(termo, "i"))
            )
        );

        return await _enderecos.Find(filter).ToListAsync();
    }

    public async Task<EnderecoEntity> UpdateAsync(EnderecoEntity endereco)
    {
        endereco.DataAtualizacao = DateTime.UtcNow;
        await _enderecos.ReplaceOneAsync(e => e.Id == endereco.Id, endereco);
        return endereco;
    }

    public async Task<IEnumerable<EnderecoEntity>> GetByFilterAsync(Func<EnderecoEntity, bool> filter)
    {
        var allEnderecos = await _enderecos.Find(e => e.Ativo).ToListAsync();
        return allEnderecos.Where(filter);
    }

    public async Task<EnderecoEntity?> GetEnderecoPrincipalByClienteAsync(string clienteId)
    {
        // Como não temos relação direta, vamos buscar todos e filtrar pelo primeiro principal
        // Em uma implementação real, você poderia ter uma coleção separada para relacionamentos
        var enderecos = await _enderecos.Find(e => e.Ativo && e.IsPrincipal).ToListAsync();
        return enderecos.FirstOrDefault();
    }

    public Task<IEnumerable<EnderecoEntity>> GetEnderecosByClienteAsync(string clienteId)
    {
        // Como não temos relação direta cliente-endereço na coleção de endereços,
        // retornamos uma lista vazia por enquanto
        return Task.FromResult<IEnumerable<EnderecoEntity>>(new List<EnderecoEntity>());
    }

    public Task<bool> DeleteEnderecosByClienteAsync(string clienteId)
    {
        // Como não temos relação direta, retornamos true
        return Task.FromResult(true);
    }

    public Task<EnderecoEntity?> GetEnderecoByClienteAndTipoAsync(string clienteId, string tipo)
    {
        // Como não temos relação direta, retornamos null
        return Task.FromResult<EnderecoEntity?>(null);
    }
}
