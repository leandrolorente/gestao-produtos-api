namespace GestaoProdutos.Domain.Enums;

public enum TipoCliente
{
    PessoaFisica = 1,
    PessoaJuridica = 2
}

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    User = 3
}

public enum StatusProduto
{
    Ativo = 1,
    Inativo = 2,
    Descontinuado = 3
}

/// <summary>
/// Status possíveis para uma venda
/// </summary>
public enum StatusVenda
{
    Pendente = 1,
    Confirmada = 2,
    Cancelada = 3,
    Finalizada = 4
}

/// <summary>
/// Formas de pagamento disponíveis
/// </summary>
public enum FormaPagamento
{
    Dinheiro = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    PIX = 4,
    Boleto = 5
}