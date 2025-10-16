using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Entidade para controle de contas a receber
/// </summary>
public class ContaReceber : BaseEntity
{
    [BsonElement("numero")]
    public string Numero { get; set; } = string.Empty; // CR-001, CR-002
    
    [BsonElement("descricao")]
    public string Descricao { get; set; } = string.Empty;
    
    [BsonElement("clienteId")]
    public string? ClienteId { get; set; }
    
    [BsonElement("clienteNome")]
    public string? ClienteNome { get; set; } // Denormalizado
    
    [BsonElement("vendaId")]
    public string? VendaId { get; set; } // Se vinculada a uma venda
    
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
    
    [BsonElement("valorRecebido")]
    public decimal ValorRecebido { get; set; }
    
    public decimal ValorRestante => ValorOriginal + Juros + Multa - Desconto - ValorRecebido;
    
    // Datas
    [BsonElement("dataEmissao")]
    public DateTime DataEmissao { get; set; }
    
    [BsonElement("dataVencimento")]
    public DateTime DataVencimento { get; set; }
    
    [BsonElement("dataRecebimento")]
    public DateTime? DataRecebimento { get; set; }
    
    // Status
    [BsonElement("status")]
    public StatusContaReceber Status { get; set; }
    
    [BsonElement("formaPagamento")]
    public FormaPagamento? FormaPagamento { get; set; }
    
    // Recorrência (assinaturas, mensalidades)
    [BsonElement("ehRecorrente")]
    public bool EhRecorrente { get; set; }
    
    [BsonElement("tipoRecorrencia")]
    public TipoRecorrencia? TipoRecorrencia { get; set; }
    
    // Observações
    [BsonElement("observacoes")]
    public string? Observacoes { get; set; }
    
    [BsonElement("vendedorId")]
    public string? VendedorId { get; set; }
    
    [BsonElement("vendedorNome")]
    public string? VendedorNome { get; set; }
    
    // Métodos de domínio
    public void Receber(decimal valor, FormaPagamento forma, DateTime? dataRecebimento = null)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");
            
        if (Status == StatusContaReceber.Recebida)
            throw new InvalidOperationException("Não é possível receber uma conta já recebida");
            
        if (Status == StatusContaReceber.Cancelada)
            throw new InvalidOperationException("Não é possível receber uma conta cancelada");
            
        if (valor > ValorRestante)
            throw new InvalidOperationException("Valor do recebimento não pode ser maior que o valor restante");
        
        ValorRecebido += valor;
        FormaPagamento = forma;
        DataRecebimento = dataRecebimento ?? DateTime.UtcNow;
        
        // Atualizar status
        if (ValorRecebido >= ValorOriginal + Juros + Multa - Desconto)
        {
            Status = StatusContaReceber.Recebida;
        }
        else if (ValorRecebido > 0)
        {
            Status = StatusContaReceber.RecebimentoParcial;
        }
        
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void Cancelar()
    {
        if (Status == StatusContaReceber.Recebida)
            throw new InvalidOperationException("Não é possível cancelar uma conta já recebida");
            
        Status = StatusContaReceber.Cancelada;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool EstaVencida()
    {
        return DateTime.UtcNow.Date > DataVencimento.Date && Status == StatusContaReceber.Vencida;
    }
    
    public bool PodeSerRecebida()
    {
        return Status != StatusContaReceber.Recebida && Status != StatusContaReceber.Cancelada;
    }
    
    public decimal CalcularJuros()
    {
        if (!EstaVencida()) return 0;
        
        var diasVencidos = (DateTime.UtcNow.Date - DataVencimento.Date).Days;
        var jurosDiarios = 0.033m / 30; // 0.033% ao dia (1% ao mês)
        return ValorOriginal * (decimal)diasVencidos * jurosDiarios;
    }
    
    public ContaReceber? GerarProximaParcela()
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
        
        return new ContaReceber
        {
            Descricao = Descricao,
            ClienteId = ClienteId,
            ClienteNome = ClienteNome,
            VendaId = VendaId,
            NotaFiscal = NotaFiscal,
            ValorOriginal = ValorOriginal,
            DataEmissao = DataVencimento,
            DataVencimento = novaDataVencimento,
            Status = StatusContaReceber.Pendente,
            EhRecorrente = EhRecorrente,
            TipoRecorrencia = TipoRecorrencia,
            Observacoes = Observacoes,
            VendedorId = VendedorId,
            VendedorNome = VendedorNome
        };
    }
    
    public void AtualizarStatus()
    {
        if (Status == StatusContaReceber.Recebida || Status == StatusContaReceber.Cancelada)
            return;
            
        if (DateTime.UtcNow.Date > DataVencimento.Date)
        {
            Status = StatusContaReceber.Vencida;
            Juros = CalcularJuros();
        }
        else if (ValorRecebido > 0)
        {
            Status = StatusContaReceber.RecebimentoParcial;
        }
        else
        {
            Status = StatusContaReceber.Pendente;
        }
    }
}
