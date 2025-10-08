using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetUsuarioPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetUsuariosPorRoleAsync(UserRole role);
    Task<bool> EmailJaExisteAsync(string email, string? usuarioId = null);
}
