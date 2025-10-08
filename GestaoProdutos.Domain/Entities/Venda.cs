using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Entidade que representa uma venda no sistema
/// </summary>
public class Venda : BaseEntity
{
    /// <summary>
    /// Número da venda (ex: VND-001)
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// ID do cliente que realizou a compra
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string ClienteId { get; set; } = string.Empty;

    /// <summary>
    /// Nome do cliente (denormalizado para performance)
    /// </summary>
    public string ClienteNome { get; set; } = string.Empty;

    /// <summary>
    /// Email do cliente (denormalizado para performance)
    /// </summary>
    public string ClienteEmail { get; set; } = string.Empty;

    /// <summary>
    /// Itens da venda
    /// </summary>
    public List<VendaItem> Items { get; set; } = new();

    /// <summary>
    /// Subtotal (soma de todos os itens)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Valor do desconto aplicado
    /// </summary>
    public decimal Desconto { get; set; }

    /// <summary>
    /// Total final da venda (subtotal - desconto)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Forma de pagamento utilizada
    /// </summary>
    public FormaPagamento FormaPagamento { get; set; }

    /// <summary>
    /// Status atual da venda
    /// </summary>
    public StatusVenda Status { get; set; }

    /// <summary>
    /// Observações adicionais sobre a venda
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Data em que a venda foi realizada
    /// </summary>
    public DateTime DataVenda { get; set; }

    /// <summary>
    /// Data de vencimento (para vendas a prazo)
    /// </summary>
    public DateTime? DataVencimento { get; set; }

    /// <summary>
    /// ID do vendedor responsável
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string? VendedorId { get; set; }

    /// <summary>
    /// Nome do vendedor (denormalizado para performance)
    /// </summary>
    public string? VendedorNome { get; set; }

    /// <summary>
    /// Construtor padrão
    /// </summary>
    public Venda()
    {
        DataVenda = DateTime.UtcNow;
        Status = StatusVenda.Pendente;
        Items = new List<VendaItem>();
    }

    /// <summary>
    /// Calcula o subtotal baseado nos itens
    /// </summary>
    public void CalcularSubtotal()
    {
        Subtotal = Items.Sum(item => item.Subtotal);
    }

    /// <summary>
    /// Calcula o total final (subtotal - desconto)
    /// </summary>
    public void CalcularTotal()
    {
        Total = Subtotal - Desconto;
    }

    /// <summary>
    /// Recalcula todos os valores da venda
    /// </summary>
    public void RecalcularValores()
    {
        // Primeiro recalcula cada item
        foreach (var item in Items)
        {
            item.CalcularSubtotal();
        }

        // Depois recalcula totais da venda
        CalcularSubtotal();
        CalcularTotal();
    }

    /// <summary>
    /// Verifica se a venda pode ser alterada
    /// </summary>
    public bool PodeSerAlterada()
    {
        return Status == StatusVenda.Pendente;
    }

    /// <summary>
    /// Confirma a venda
    /// </summary>
    public void Confirmar()
    {
        if (Status != StatusVenda.Pendente)
            throw new InvalidOperationException("Apenas vendas pendentes podem ser confirmadas");

        Status = StatusVenda.Confirmada;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Finaliza a venda
    /// </summary>
    public void Finalizar()
    {
        if (Status != StatusVenda.Confirmada)
            throw new InvalidOperationException("Apenas vendas confirmadas podem ser finalizadas");

        Status = StatusVenda.Finalizada;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela a venda
    /// </summary>
    public void Cancelar()
    {
        if (Status == StatusVenda.Finalizada)
            throw new InvalidOperationException("Vendas finalizadas não podem ser canceladas");

        Status = StatusVenda.Cancelada;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a venda está vencida
    /// </summary>
    public bool EstaVencida()
    {
        return DataVencimento.HasValue && 
               DataVencimento.Value.Date < DateTime.UtcNow.Date && 
               Status == StatusVenda.Pendente;
    }
}

/// <summary>
/// Representa um item de uma venda
/// </summary>
public class VendaItem
{
    /// <summary>
    /// ID único do item na venda
    /// </summary>
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// ID do produto
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProdutoId { get; set; } = string.Empty;

    /// <summary>
    /// Nome do produto (denormalizado para performance)
    /// </summary>
    public string ProdutoNome { get; set; } = string.Empty;

    /// <summary>
    /// SKU do produto (denormalizado para performance)
    /// </summary>
    public string ProdutoSku { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade vendida
    /// </summary>
    public int Quantidade { get; set; }

    /// <summary>
    /// Preço unitário no momento da venda
    /// </summary>
    public decimal PrecoUnitario { get; set; }

    /// <summary>
    /// Subtotal do item (quantidade * preço unitário)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Construtor padrão
    /// </summary>
    public VendaItem()
    {
    }

    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public VendaItem(string produtoId, string produtoNome, string produtoSku, int quantidade, decimal precoUnitario)
    {
        ProdutoId = produtoId;
        ProdutoNome = produtoNome;
        ProdutoSku = produtoSku;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        CalcularSubtotal();
    }

    /// <summary>
    /// Calcula o subtotal do item
    /// </summary>
    public void CalcularSubtotal()
    {
        Subtotal = Quantidade * PrecoUnitario;
    }

    /// <summary>
    /// Atualiza a quantidade e recalcula o subtotal
    /// </summary>
    public void AtualizarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        Quantidade = novaQuantidade;
        CalcularSubtotal();
    }

    /// <summary>
    /// Atualiza o preço unitário e recalcula o subtotal
    /// </summary>
    public void AtualizarPrecoUnitario(decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new ArgumentException("Preço deve ser maior que zero");

        PrecoUnitario = novoPreco;
        CalcularSubtotal();
    }
}
