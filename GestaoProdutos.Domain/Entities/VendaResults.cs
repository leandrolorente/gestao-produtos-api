namespace GestaoProdutos.Domain.Entities;

/// <summary>
/// Classe para retorno de dados de top clientes
/// </summary>
public class TopClienteResult
{
    public string ClienteNome { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal ValorTotal { get; set; }
}

/// <summary>
/// Classe para retorno de vendas por mês
/// </summary>
public class VendasPorMesResult
{
    public string Mes { get; set; } = string.Empty;
    public int Vendas { get; set; }
    public decimal Faturamento { get; set; }
}