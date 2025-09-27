using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Infrastructure.Data;
using MongoDB.Driver;

namespace GestaoProdutos.Infrastructure.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(MongoDbContext context) : base(context) { }

    public async Task<Cliente?> GetClientePorCpfCnpjAsync(string cpfCnpj)
    {
        return await _collection
            .Find(c => c.CpfCnpj.Valor == cpfCnpj)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Cliente>> GetClientesAtivosPorTipoAsync(TipoCliente tipo)
    {
        return await _collection
            .Find(c => c.Ativo && c.Tipo == tipo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cliente>> GetClientesComCompraRecenteAsync(int dias = 30)
    {
        var dataLimite = DateTime.UtcNow.AddDays(-dias);
        return await _collection
            .Find(c => c.Ativo && c.UltimaCompra != null && c.UltimaCompra >= dataLimite)
            .ToListAsync();
    }

    public async Task<bool> CpfCnpjJaExisteAsync(string cpfCnpj, string? clienteId = null)
    {
        var filter = Builders<Cliente>.Filter.Eq(c => c.CpfCnpj.Valor, cpfCnpj);

        if (!string.IsNullOrEmpty(clienteId))
        {
            filter = Builders<Cliente>.Filter.And(
                filter,
                Builders<Cliente>.Filter.Ne(c => c.Id, clienteId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }
}

public class UsuarioRepository : BaseRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(MongoDbContext context) : base(context) { }

    public async Task<Usuario?> GetUsuarioPorEmailAsync(string email)
    {
        return await _collection
            .Find(u => u.Email.Valor == email && u.Ativo)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Usuario>> GetUsuariosPorRoleAsync(UserRole role)
    {
        return await _collection
            .Find(u => u.Ativo && u.Role == role)
            .ToListAsync();
    }

    public async Task<bool> EmailJaExisteAsync(string email, string? usuarioId = null)
    {
        var filter = Builders<Usuario>.Filter.And(
            Builders<Usuario>.Filter.Eq(u => u.Email.Valor, email),
            Builders<Usuario>.Filter.Eq(u => u.Ativo, true)
        );

        if (!string.IsNullOrEmpty(usuarioId))
        {
            filter = Builders<Usuario>.Filter.And(
                filter,
                Builders<Usuario>.Filter.Ne(u => u.Id, usuarioId)
            );
        }

        var count = await _collection.CountDocumentsAsync(filter);
        return count > 0;
    }
}