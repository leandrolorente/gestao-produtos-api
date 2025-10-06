namespace GestaoProdutos.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProdutoRepository Produtos { get; }
    IClienteRepository Clientes { get; }
    IFornecedorRepository Fornecedores { get; }
    IEnderecoRepository Enderecos { get; }
    IUsuarioRepository Usuarios { get; }
    IVendaRepository Vendas { get; }
    Task<bool> SaveChangesAsync();
}