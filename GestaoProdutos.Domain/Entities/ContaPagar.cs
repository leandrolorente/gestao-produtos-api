using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Entidade para controle de contas a pagar
/// </summary>
public class ContaPagar : BaseEntity
{
    [BsonElement("numero")]
    public string Numero { get; set; } = string.Empty; // CP-001, CP-002
    
    [BsonElement("descricao")]
    public string Descricao { get; set; } = string.Empty;
    
    [BsonElement("fornecedorId")]
    public string? FornecedorId { get; set; }
    
    [BsonElement("fornecedorNome")]
    public string? FornecedorNome { get; set; } // Denormalizado
    
    [BsonElement("compraId")]
    public string? CompraId { get; set; } // Se vinculada a uma compra
    
    [BsonElement("notaFiscal")]
    public string? NotaFiscal { get; set; }
    
    // Valores
    [BsonElement("valorOriginal")]
    public decimal ValorOriginal { get; set; }
    
    [BsonElement("desconto")]
    public decimal Desconto { get; set; }
    
    [BsonElement("juros")]
    public decimal Juros { get; set; }
    
    [BsonElement("multa")]
    public decimal Multa { get; set; }
    
    [BsonElement("valorPago")]
    public decimal ValorPago { get; set; }
    
    public decimal ValorRestante => ValorOriginal + Juros + Multa - Desconto - ValorPago;
    
    // Datas
    [BsonElement("dataEmissao")]
    public DateTime DataEmissao { get; set; }
    
    [BsonElement("dataVencimento")]
    public DateTime DataVencimento { get; set; }
    
    [BsonElement("dataPagamento")]
    public DateTime? DataPagamento { get; set; }
    
    // Status e categoria
    [BsonElement("status")]
    public StatusContaPagar Status { get; set; }
    
    [BsonElement("categoria")]
    public CategoriaConta Categoria { get; set; }
    
    [BsonElement("formaPagamento")]
    public FormaPagamento? FormaPagamento { get; set; }
    
    // Recorrência
    [BsonElement("ehRecorrente")]
    public bool EhRecorrente { get; set; }
    
    [BsonElement("tipoRecorrencia")]
    public TipoRecorrencia? TipoRecorrencia { get; set; }
    
    [BsonElement("diasRecorrencia")]
    public int? DiasRecorrencia { get; set; }
    
    // Observações
    [BsonElement("observacoes")]
    public string? Observacoes { get; set; }
    
    [BsonElement("centroCusto")]
    public string? CentroCusto { get; set; }
    
    // Métodos de domínio
    public void Pagar(decimal valor, FormaPagamento forma, DateTime? dataPagamento = null)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");
            
        if (Status == StatusContaPagar.Paga)
            throw new InvalidOperationException("Não é possível pagar uma conta já paga");
            
        if (Status == StatusContaPagar.Cancelada)
            throw new InvalidOperationException("Não é possível pagar uma conta cancelada");
            
        if (valor > ValorRestante)
            throw new InvalidOperationException("Valor do pagamento não pode ser maior que o valor restante");
        
        ValorPago += valor;
        FormaPagamento = forma;
        DataPagamento = dataPagamento ?? DateTime.UtcNow;
        
        // Atualizar status
        if (ValorPago >= ValorOriginal + Juros + Multa - Desconto)
        {
            Status = StatusContaPagar.Paga;
        }
        else if (ValorPago > 0)
        {
            Status = StatusContaPagar.PagamentoParcial;
        }
        
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void Cancelar()
    {
        if (Status == StatusContaPagar.Paga)
            throw new InvalidOperationException("Não é possível cancelar uma conta já paga");
            
        Status = StatusContaPagar.Cancelada;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool EstaVencida()
    {
        return DateTime.UtcNow.Date > DataVencimento.Date && Status == StatusContaPagar.Vencida;
    }
    
    public bool PodeSerPaga()
    {
        return Status != StatusContaPagar.Paga && Status != StatusContaPagar.Cancelada;
    }
    
    public decimal CalcularJuros()
    {
        if (!EstaVencida()) return 0;
        
        var diasVencidos = (DateTime.UtcNow.Date - DataVencimento.Date).Days;
        var jurosDiarios = 0.033m / 30; // 0.033% ao dia (1% ao mês)
        return ValorOriginal * (decimal)diasVencidos * jurosDiarios;
    }
    
    public ContaPagar? GerarProximaParcela()
    {
        if (!EhRecorrente || !TipoRecorrencia.HasValue)
            return null;
            
        var novaDataVencimento = TipoRecorrencia.Value switch
        {
            Domain.Enums.TipoRecorrencia.Semanal => DataVencimento.AddDays(7),
            Domain.Enums.TipoRecorrencia.Quinzenal => DataVencimento.AddDays(15),
            Domain.Enums.TipoRecorrencia.Mensal => DataVencimento.AddMonths(1),
            Domain.Enums.TipoRecorrencia.Bimestral => DataVencimento.AddMonths(2),
            Domain.Enums.TipoRecorrencia.Trimestral => DataVencimento.AddMonths(3),
            Domain.Enums.TipoRecorrencia.Anual => DataVencimento.AddYears(1),
            _ => DataVencimento.AddMonths(1)
        };
        
        return new ContaPagar
        {
            Descricao = Descricao,
            FornecedorId = FornecedorId,
            FornecedorNome = FornecedorNome,
            ValorOriginal = ValorOriginal,
            DataEmissao = DataVencimento,
            DataVencimento = novaDataVencimento,
            Status = StatusContaPagar.Pendente,
            Categoria = Categoria,
            EhRecorrente = EhRecorrente,
            TipoRecorrencia = TipoRecorrencia,
            Observacoes = Observacoes,
            CentroCusto = CentroCusto
        };
    }
    
    public void AtualizarStatus()
    {
        if (Status == StatusContaPagar.Paga || Status == StatusContaPagar.Cancelada)
            return;
            
        if (DateTime.UtcNow.Date > DataVencimento.Date)
        {
            Status = StatusContaPagar.Vencida;
            Juros = CalcularJuros();
        }
        else if (ValorPago > 0)
        {
            Status = StatusContaPagar.PagamentoParcial;
        }
        else
        {
            Status = StatusContaPagar.Pendente;
        }
    }
}
