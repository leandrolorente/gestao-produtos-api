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
    
    [BsonElement("enderecoId")]
    public string? EnderecoId { get; set; } // Referência para a entidade Endereco
    
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

    public void AtualizarInformacoes(string nome, string email, string telefone, string cpfCnpj)
    {
        Nome = nome;
        Email = string.IsNullOrWhiteSpace(email) ? null : new Email(email);
        Telefone = telefone;
        CpfCnpj = string.IsNullOrWhiteSpace(cpfCnpj) ? null : new CpfCnpj(cpfCnpj);
        
        // Atualizar o tipo automaticamente baseado no CPF/CNPJ
        if (CpfCnpj != null)
        {
            Tipo = CpfCnpj.EhCpf ? TipoCliente.PessoaFisica : TipoCliente.PessoaJuridica;
        }
        
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool TemCompraRecente(int dias = 30)
    {
        return UltimaCompra.HasValue && 
               UltimaCompra.Value >= DateTime.UtcNow.AddDays(-dias);
    }
}