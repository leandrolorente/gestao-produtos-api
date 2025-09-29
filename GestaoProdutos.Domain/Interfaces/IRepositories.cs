using GestaoProdutos.Domain.Entities;

namespace GestaoProdutos.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByFilterAsync(Func<T, bool> predicate);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutosComEstoqueBaixoAsync();
    Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(string categoria);
    Task<Produto?> GetProdutoPorSkuAsync(string sku);
    Task<Produto?> GetProdutoPorBarcodeAsync(string barcode);
    Task<bool> SkuJaExisteAsync(string sku, string? produtoId = null);
    Task<bool> BarcodeJaExisteAsync(string barcode, string? produtoId = null);
}

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetClientePorCpfCnpjAsync(string cpfCnpj);
    Task<IEnumerable<Cliente>> GetClientesAtivosPorTipoAsync(Domain.Enums.TipoCliente tipo);
    Task<IEnumerable<Cliente>> GetClientesComCompraRecenteAsync(int dias = 30);
    Task<bool> CpfCnpjJaExisteAsync(string cpfCnpj, string? clienteId = null);
}

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetUsuarioPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetUsuariosPorRoleAsync(Domain.Enums.UserRole role);
    Task<bool> EmailJaExisteAsync(string email, string? usuarioId = null);
}

public interface IUnitOfWork : IDisposable
{
    IProdutoRepository Produtos { get; }
    IClienteRepository Clientes { get; }
    IUsuarioRepository Usuarios { get; }
    Task<bool> SaveChangesAsync();
}