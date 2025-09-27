using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;

namespace GestaoProdutos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private IProdutoRepository? _produtos;
    private IClienteRepository? _clientes;
    private IUsuarioRepository? _usuarios;

    public UnitOfWork(MongoDbContext context)
    {
        _context = context;
    }

    public IProdutoRepository Produtos => 
        _produtos ??= new ProdutoRepository(_context);

    public IClienteRepository Clientes => 
        _clientes ??= new ClienteRepository(_context);

    public IUsuarioRepository Usuarios => 
        _usuarios ??= new UsuarioRepository(_context);

    public async Task<bool> SaveChangesAsync()
    {
        // MongoDB não precisa de transações explícitas para operações simples
        // Para operações complexas, podemos implementar transações aqui
        return await Task.FromResult(true);
    }

    public void Dispose()
    {
        // MongoDB driver gerencia as conexões automaticamente
        GC.SuppressFinalize(this);
    }
}