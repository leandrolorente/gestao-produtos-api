using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public Email? Email { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public CpfCnpj? CpfCnpj { get; set; }
    public Endereco Endereco { get; set; } = new Endereco();
    public TipoCliente Tipo { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public string? Observacoes { get; set; }
    
    // Propriedades derivadas
    public bool EhPessoaFisica => Tipo == TipoCliente.PessoaFisica;
    public bool EhPessoaJuridica => Tipo == TipoCliente.PessoaJuridica;
    
    // Métodos de domínio
    public void AtualizarUltimaCompra()
    {
        UltimaCompra = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void AtualizarInformacoes(string nome, string email, string telefone)
    {
        Nome = nome;
        Email = new Email(email);
        Telefone = telefone;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool TemCompraRecente(int dias = 30)
    {
        return UltimaCompra.HasValue && 
               UltimaCompra.Value >= DateTime.UtcNow.AddDays(-dias);
    }
}