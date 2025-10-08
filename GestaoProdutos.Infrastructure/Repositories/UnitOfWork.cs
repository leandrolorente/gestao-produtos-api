using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;

namespace GestaoProdutos.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private IProdutoRepository? _produtos;
    private IClienteRepository? _clientes;
    private IEnderecoRepository? _enderecos;
    private IUsuarioRepository? _usuarios;
    private IVendaRepository? _vendas;
    private IFornecedorRepository? _fornecedores;
    private IContaPagarRepository? _contasPagar;
    private IContaReceberRepository? _contasReceber;

    public UnitOfWork(MongoDbContext context)
    {
        _context = context;
    }

    public IProdutoRepository Produtos => 
        _produtos ??= new ProdutoRepository(_context);

    public IClienteRepository Clientes => 
        _clientes ??= new ClienteRepository(_context);

    public IEnderecoRepository Enderecos => 
        _enderecos ??= new EnderecoRepository(_context);

    public IUsuarioRepository Usuarios => 
        _usuarios ??= new UsuarioRepository(_context);

    public IVendaRepository Vendas => 
        _vendas ??= new VendaRepository(_context);

    public IFornecedorRepository Fornecedores => 
        _fornecedores ??= new FornecedorRepository(_context);

    public IContaPagarRepository ContasPagar => 
        _contasPagar ??= new ContaPagarRepository(_context);

    public IContaReceberRepository ContasReceber => 
        _contasReceber ??= new ContaReceberRepository(_context);

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
