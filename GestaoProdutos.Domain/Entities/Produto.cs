using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

public abstract class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}

public class Produto : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
    public StatusProduto Status { get; set; } = StatusProduto.Ativo;
    public string? Descricao { get; set; }
    public string? Categoria { get; set; }
    public decimal? PrecoCompra { get; set; }
    public int? EstoqueMinimo { get; set; }
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
    
    // Métodos de domínio
    public bool EstaComEstoqueBaixo()
    {
        return EstoqueMinimo.HasValue && Quantidade <= EstoqueMinimo;
    }
    
    public void AtualizarEstoque(int novaQuantidade)
    {
        Quantidade = novaQuantidade;
        UltimaAtualizacao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero");
            
        Preco = novoPreco;
        UltimaAtualizacao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }
}