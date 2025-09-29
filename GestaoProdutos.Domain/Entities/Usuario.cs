 using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public Email Email { get; set; } = new Email(string.Empty);
    public UserRole Role { get; set; }
    public string Avatar { get; set; } = string.Empty; // Removido nullable para garantir sempre ter um avatar
    public string Departamento { get; set; } = string.Empty;
    public DateTime? UltimoLogin { get; set; }
    public string SenhaHash { get; set; } = string.Empty;
    
    // Métodos de domínio
    public bool EhAdministrador => Role == UserRole.Admin;
    public bool EhGerente => Role == UserRole.Manager;
    public bool EhUsuario => Role == UserRole.User;
    
    public void RegistrarLogin()
    {
        UltimoLogin = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool PodeGerenciarProdutos()
    {
        return Role == UserRole.Admin || Role == UserRole.Manager;
    }
    
    public bool PodeGerenciarUsuarios()
    {
        return Role == UserRole.Admin;
    }
}