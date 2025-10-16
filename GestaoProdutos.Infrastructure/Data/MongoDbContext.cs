using MongoDB.Driver;
using GestaoProdutos.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace GestaoProdutos.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        
        // Configurar convenções do MongoDB
        ConfigureConventions();
    }

    private static void ConfigureConventions()
    {
        // Configurar para ignorar campos extras durante deserialização
        var conventionPack = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        };
        
        ConventionRegistry.Register("MyConventions", conventionPack, t => true);
        
        // Configurar mapeamento específico para Cliente se necessário
        if (!BsonClassMap.IsClassMapRegistered(typeof(Cliente)))
        {
            BsonClassMap.RegisterClassMap<Cliente>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }

    public IMongoCollection<Produto> Produtos => _database.GetCollection<Produto>("produtos");
    public IMongoCollection<Cliente> Clientes => _database.GetCollection<Cliente>("clientes");
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("usuarios");
    public IMongoCollection<Venda> Vendas => _database.GetCollection<Venda>("vendas");
    public IMongoCollection<EnderecoEntity> Enderecos => _database.GetCollection<EnderecoEntity>("enderecos");
    public IMongoCollection<Fornecedor> Fornecedores => _database.GetCollection<Fornecedor>("fornecedores");
    public IMongoCollection<ContaPagar> ContasPagar => _database.GetCollection<ContaPagar>("contasPagar");
    public IMongoCollection<ContaReceber> ContasReceber => _database.GetCollection<ContaReceber>("contasReceber");

    // Método genérico para acessar qualquer collection
    public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name);

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

        // Fornecedores - índices otimizados
        var fornecedoresIndexes = new List<CreateIndexModel<Fornecedor>>
        {
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.CnpjCpf)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.RazaoSocial)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.NomeFantasia)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.Email)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.Tipo)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.Status)),
            new(Builders<Fornecedor>.IndexKeys.Ascending(f => f.UltimaCompra)),
            new(Builders<Fornecedor>.IndexKeys.Text(f => f.RazaoSocial).Text(f => f.NomeFantasia))
        };
        await Fornecedores.Indexes.CreateManyAsync(fornecedoresIndexes);
    }
}
