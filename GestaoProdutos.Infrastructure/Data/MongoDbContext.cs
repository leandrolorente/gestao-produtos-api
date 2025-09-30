using MongoDB.Driver;
using GestaoProdutos.Domain.Entities;

namespace GestaoProdutos.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Produto> Produtos => _database.GetCollection<Produto>("produtos");
    public IMongoCollection<Cliente> Clientes => _database.GetCollection<Cliente>("clientes");
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("usuarios");
    public IMongoCollection<Venda> Vendas => _database.GetCollection<Venda>("vendas");

    // Método para criação de índices
    public async Task CreateIndexesAsync()
    {
        // Produtos - índice no SKU
        var produtosIndexes = new List<CreateIndexModel<Produto>>
        {
            new(Builders<Produto>.IndexKeys.Ascending(p => p.Sku)),
            new(Builders<Produto>.IndexKeys.Ascending(p => p.Nome)),
            new(Builders<Produto>.IndexKeys.Ascending(p => p.Categoria))
        };
        await Produtos.Indexes.CreateManyAsync(produtosIndexes);

        // Clientes - índice no CPF/CNPJ
        var clientesIndexes = new List<CreateIndexModel<Cliente>>
        {
            new(Builders<Cliente>.IndexKeys.Ascending(c => c.CpfCnpj)),
            new(Builders<Cliente>.IndexKeys.Ascending(c => c.Email)),
            new(Builders<Cliente>.IndexKeys.Ascending(c => c.Nome))
        };
        await Clientes.Indexes.CreateManyAsync(clientesIndexes);

        // Usuários - índice no email
        var usuariosIndexes = new List<CreateIndexModel<Usuario>>
        {
            new(Builders<Usuario>.IndexKeys.Ascending(u => u.Email)),
            new(Builders<Usuario>.IndexKeys.Ascending(u => u.Nome))
        };
        await Usuarios.Indexes.CreateManyAsync(usuariosIndexes);
    }
}