using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Entidade que representa um fornecedor no sistema
/// </summary>
public class Fornecedor : BaseEntity
{
    /// <summary>
    /// Razão social do fornecedor
    /// </summary>
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// Nome fantasia do fornecedor
    /// </summary>
    public string? NomeFantasia { get; set; }

    /// <summary>
    /// CNPJ ou CPF do fornecedor
    /// </summary>
    public CpfCnpj CnpjCpf { get; set; } = null!;

    /// <summary>
    /// Email do fornecedor
    /// </summary>
    public Email Email { get; set; } = null!;

    /// <summary>
    /// Telefone principal do fornecedor
    /// </summary>
    public string Telefone { get; set; } = string.Empty;

    /// <summary>
    /// ID do endereço do fornecedor (referência para a entidade Endereco)
    /// </summary>
    [BsonElement("enderecoId")]
    public string? EnderecoId { get; set; }

    /// <summary>
    /// Endereço do fornecedor (DEPRECATED - usar EnderecoId)
    /// </summary>
    [BsonIgnore]
    public Endereco? Endereco { get; set; }

    /// <summary>
    /// Inscrição estadual
    /// </summary>
    public string? InscricaoEstadual { get; set; }

    /// <summary>
    /// Inscrição municipal
    /// </summary>
    public string? InscricaoMunicipal { get; set; }

    /// <summary>
    /// Tipo do fornecedor (Nacional, Internacional)
    /// </summary>
    public TipoFornecedor Tipo { get; set; } = TipoFornecedor.Nacional;

    /// <summary>
    /// Status do fornecedor (Ativo, Inativo, Bloqueado)
    /// </summary>
    public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;

    /// <summary>
    /// Observações sobre o fornecedor
    /// </summary>
    public string? Observacoes { get; set; }

    /// <summary>
    /// Nome do contato principal
    /// </summary>
    public string? ContatoPrincipal { get; set; }

    /// <summary>
    /// Site do fornecedor
    /// </summary>
    public string? Site { get; set; }

    // === DADOS BANCÁRIOS ===
    /// <summary>
    /// Nome do banco
    /// </summary>
    public string? Banco { get; set; }

    /// <summary>
    /// Agência bancária
    /// </summary>
    public string? Agencia { get; set; }

    /// <summary>
    /// Conta bancária
    /// </summary>
    public string? Conta { get; set; }

    /// <summary>
    /// Chave PIX
    /// </summary>
    public string? Pix { get; set; }

    // === CONDIÇÕES COMERCIAIS ===
    /// <summary>
    /// Prazo de pagamento padrão em dias
    /// </summary>
    public int PrazoPagamentoPadrao { get; set; } = 30;

    /// <summary>
    /// Limite de crédito concedido
    /// </summary>
    public decimal LimiteCredito { get; set; } = 0;

    /// <summary>
    /// Condições de pagamento detalhadas
    /// </summary>
    public string? CondicoesPagamento { get; set; }

    // === RELACIONAMENTOS E ESTATÍSTICAS ===
    /// <summary>
    /// IDs dos produtos fornecidos
    /// </summary>
    public List<string> ProdutoIds { get; set; } = new();

    /// <summary>
    /// Data da última compra realizada
    /// </summary>
    public DateTime? UltimaCompra { get; set; }

    /// <summary>
    /// Total comprado do fornecedor (histórico)
    /// </summary>
    public decimal TotalComprado { get; set; } = 0;

    /// <summary>
    /// Quantidade de compras realizadas
    /// </summary>
    public int QuantidadeCompras { get; set; } = 0;

    // === MÉTODOS DE DOMÍNIO ===

    /// <summary>
    /// Verifica se o fornecedor está ativo
    /// </summary>
    public bool EstaAtivo() => Status == StatusFornecedor.Ativo && Ativo;

    /// <summary>
    /// Verifica se o fornecedor está bloqueado
    /// </summary>
    public bool EstaBloqueado() => Status == StatusFornecedor.Bloqueado;

    /// <summary>
    /// Bloqueia o fornecedor
    /// </summary>
    public void Bloquear(string? motivo = null)
    {
        Status = StatusFornecedor.Bloqueado;
        if (!string.IsNullOrEmpty(motivo))
        {
            Observacoes = $"{Observacoes}\n[BLOQUEADO] {DateTime.UtcNow:dd/MM/yyyy}: {motivo}".Trim();
        }
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Desbloqueia o fornecedor
    /// </summary>
    public void Desbloquear()
    {
        Status = StatusFornecedor.Ativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Inativa o fornecedor
    /// </summary>
    public void Inativar()
    {
        Status = StatusFornecedor.Inativo;
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Adiciona um produto ao fornecedor
    /// </summary>
    public void AdicionarProduto(string produtoId)
    {
        if (!string.IsNullOrEmpty(produtoId) && !ProdutoIds.Contains(produtoId))
        {
            ProdutoIds.Add(produtoId);
            DataAtualizacao = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Remove um produto do fornecedor
    /// </summary>
    public void RemoverProduto(string produtoId)
    {
        if (!string.IsNullOrEmpty(produtoId) && ProdutoIds.Contains(produtoId))
        {
            ProdutoIds.Remove(produtoId);
            DataAtualizacao = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Registra uma nova compra
    /// </summary>
    public void RegistrarCompra(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("Valor da compra deve ser maior que zero");
        }
        
        TotalComprado += valor;
        QuantidadeCompras++;
        UltimaCompra = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se tem crédito disponível
    /// </summary>
    public bool TemCreditoDisponivel(decimal valor) => LimiteCredito == 0 || valor <= LimiteCredito;

    /// <summary>
    /// Calcula o ticket médio de compras
    /// </summary>
    public decimal CalcularTicketMedio() => QuantidadeCompras > 0 ? TotalComprado / QuantidadeCompras : 0;

    /// <summary>
    /// Verifica se é um fornecedor frequente (5 ou mais compras)
    /// </summary>
    public bool EhFrequente() => QuantidadeCompras >= 5;

    /// <summary>
    /// Atualiza dados básicos do fornecedor
    /// </summary>
    public void AtualizarDados(string razaoSocial, string? nomeFantasia, string telefone, string? contatoPrincipal)
    {
        RazaoSocial = razaoSocial;
        NomeFantasia = nomeFantasia;
        Telefone = telefone;
        ContatoPrincipal = contatoPrincipal;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza condições comerciais
    /// </summary>
    public void AtualizarCondicoesComerciais(int prazoPagamento, decimal limiteCredito, string? condicoesPagamento)
    {
        PrazoPagamentoPadrao = prazoPagamento;
        LimiteCredito = limiteCredito;
        CondicoesPagamento = condicoesPagamento;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza dados bancários
    /// </summary>
    public void AtualizarDadosBancarios(string? banco, string? agencia, string? conta, string? pix)
    {
        Banco = banco;
        Agencia = agencia;
        Conta = conta;
        Pix = pix;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o fornecedor está com limite de crédito excedido
    /// </summary>
    /// <returns>True se o limite foi excedido</returns>
    public bool EstaComLimiteExcedido()
    {
        return LimiteCredito > 0 && TotalComprado > LimiteCredito;
    }

    /// <summary>
    /// Verifica se o fornecedor pode realizar compras
    /// </summary>
    /// <returns>True se pode comprar</returns>
    public bool PodeComprar()
    {
        return Status == StatusFornecedor.Ativo && Ativo;
    }

    /// <summary>
    /// Verifica se houve compra recente no período especificado
    /// </summary>
    /// <param name="dias">Número de dias para considerar como recente</param>
    /// <returns>True se houve compra recente</returns>
    public bool TemCompraRecente(int dias = 90)
    {
        return UltimaCompra.HasValue && UltimaCompra.Value >= DateTime.UtcNow.AddDays(-dias);
    }

    /// <summary>
    /// Obtém um resumo comercial do fornecedor
    /// </summary>
    /// <returns>String com resumo das informações comerciais</returns>
    public string ObterResumoComercial()
    {
        return $"Fornecedor: {RazaoSocial}\n" +
               $"Total comprado: {TotalComprado:C}\n" +
               $"Quantidade de compras: {QuantidadeCompras}\n" +
               $"Ticket médio: {CalcularTicketMedio():C}\n" +
               $"Status: {Status}\n" +
               $"É frequente: {(EhFrequente() ? "Sim" : "Não")}\n" +
               $"Última compra: {(UltimaCompra?.ToString("dd/MM/yyyy") ?? "Nunca")}";
    }
}
