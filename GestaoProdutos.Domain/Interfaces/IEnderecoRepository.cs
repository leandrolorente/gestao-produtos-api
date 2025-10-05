using GestaoProdutos.Domain.Entities;

namespace GestaoProdutos.Domain.Interfaces;

public interface IEnderecoRepository : IRepository<EnderecoEntity>
{
    Task<EnderecoEntity?> GetEnderecoPrincipalByClienteAsync(string clienteId);
    Task<IEnumerable<EnderecoEntity>> GetEnderecosByClienteAsync(string clienteId);
    Task<bool> DeleteEnderecosByClienteAsync(string clienteId);
    Task<EnderecoEntity?> GetEnderecoByClienteAndTipoAsync(string clienteId, string tipo);
}