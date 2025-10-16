namespace GestaoProdutos.Domain.Enums;

/// <summary>
/// Status das contas a pagar
/// </summary>
public enum StatusContaPagar
{
    Pendente = 1,
    Paga = 2,
    Cancelada = 3,
    Vencida = 4,
    PagamentoParcial = 5
}
