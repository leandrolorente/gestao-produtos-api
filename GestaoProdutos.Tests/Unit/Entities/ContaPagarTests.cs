using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using Xunit;

namespace GestaoProdutos.Tests.Unit.Entities;

/// <summary>
/// Testes unitários para a entidade ContaPagar
/// </summary>
public class ContaPagarTests
{
    [Fact]
    public void ContaPagar_CriarNova_DeveDefinirValoresIniciais()
    {
        // Arrange & Act
        var conta = new ContaPagar
        {
            Descricao = "Fornecedor XYZ - Material de escritório",
            FornecedorId = ObjectId.GenerateNewId().ToString(),
            FornecedorNome = "Fornecedor XYZ",
            ValorOriginal = 1000.00M,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Status = StatusContaPagar.Pendente,
            Categoria = CategoriaConta.Fornecedores
        };

        // Assert
        conta.Descricao.Should().Be("Fornecedor XYZ - Material de escritório");
        conta.FornecedorNome.Should().Be("Fornecedor XYZ");
        conta.ValorOriginal.Should().Be(1000.00M);
        conta.Status.Should().Be(StatusContaPagar.Pendente);
        conta.Categoria.Should().Be(CategoriaConta.Fornecedores);
        conta.ValorPago.Should().Be(0);
        conta.ValorRestante.Should().Be(1000.00M);
        conta.Juros.Should().Be(0);
        conta.Multa.Should().Be(0);
        conta.Desconto.Should().Be(0);
    }

    [Fact]
    public void ContaPagar_Pagar_ValorTotal_DeveMarcarComoPaga()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30)
        };

        // Act
        conta.Pagar(1000.00M, FormaPagamento.PIX, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaPagar.Paga);
        conta.ValorPago.Should().Be(1000.00M);
        conta.ValorRestante.Should().Be(0);
        conta.DataPagamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        conta.FormaPagamento.Should().Be(FormaPagamento.PIX);
    }

    [Fact]
    public void ContaPagar_Pagar_ValorParcial_DeveMarcarComoPagamentoParcial()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30)
        };

        // Act
        conta.Pagar(300.00M, FormaPagamento.PIX, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaPagar.PagamentoParcial);
        conta.ValorPago.Should().Be(300.00M);
        conta.ValorRestante.Should().Be(700.00M);
    }

    [Fact]
    public void ContaPagar_Pagar_ContaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Paga
        };

        // Act & Assert
        var act = () => conta.Pagar(100.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível pagar uma conta já paga");
    }

    [Fact]
    public void ContaPagar_Pagar_ContaCancelada_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Cancelada
        };

        // Act & Assert
        var act = () => conta.Pagar(100.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível pagar uma conta cancelada");
    }

    [Fact]
    public void ContaPagar_Pagar_ValorMaiorQueRestante_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            ValorPago = 500.00M,
            Status = StatusContaPagar.PagamentoParcial
        };

        // Act & Assert
        var act = () => conta.Pagar(600.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Valor do pagamento não pode ser maior que o valor restante");
    }

    [Fact]
    public void ContaPagar_Cancelar_ContaPendente_DeveCancelar()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Pendente
        };

        // Act
        conta.Cancelar();

        // Assert
        conta.Status.Should().Be(StatusContaPagar.Cancelada);
    }

    [Fact]
    public void ContaPagar_Cancelar_ContaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Paga
        };

        // Act & Assert
        var act = () => conta.Cancelar();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível cancelar uma conta já paga");
    }

    [Theory]
    [InlineData(StatusContaPagar.Pendente, false)]
    [InlineData(StatusContaPagar.Vencida, true)]
    [InlineData(StatusContaPagar.PagamentoParcial, false)]
    [InlineData(StatusContaPagar.Paga, false)]
    [InlineData(StatusContaPagar.Cancelada, false)]
    public void ContaPagar_EstaVencida_DeveRetornarStatusCorreto(StatusContaPagar status, bool esperado)
    {
        // Arrange
        var conta = new ContaPagar
        {
            Status = status,
            DataVencimento = DateTime.UtcNow.AddDays(-1) // Data no passado
        };

        // Act
        var resultado = conta.EstaVencida();

        // Assert
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void ContaPagar_CalcularJuros_ContaVencida_DeveCalcularJuros()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(-10), // 10 dias em atraso
            Status = StatusContaPagar.Vencida
        };

        // Act
        var juros = conta.CalcularJuros();

        // Assert
        juros.Should().BeGreaterThan(0); // Deveria ter juros para conta vencida
    }

    [Fact]
    public void ContaPagar_CalcularJuros_ContaNaoVencida_DeveRetornarZero()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(10),
            Status = StatusContaPagar.Pendente
        };

        // Act
        var juros = conta.CalcularJuros();

        // Assert
        juros.Should().Be(0);
    }

    [Fact]
    public void ContaPagar_AtualizarStatus_ContaVencida_DeveMarcarComoVencida()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(-1), // Vencida
            Status = StatusContaPagar.Pendente
        };

        // Act
        conta.AtualizarStatus();

        // Assert
        conta.Status.Should().Be(StatusContaPagar.Vencida);
    }

    [Fact]
    public void ContaPagar_AtualizarStatus_ContaNaoVencida_DeveManterPendente()
    {
        // Arrange
        var conta = new ContaPagar
        {
            ValorOriginal = 1000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(10),
            Status = StatusContaPagar.Pendente
        };

        // Act
        conta.AtualizarStatus();

        // Assert
        conta.Status.Should().Be(StatusContaPagar.Pendente);
    }

    [Fact]
    public void ContaPagar_GerarProximaParcela_ContaRecorrente_DeveGerarNovaConta()
    {
        // Arrange
        var conta = new ContaPagar
        {
            Descricao = "Aluguel - Janeiro",
            ValorOriginal = 1000.00M,
            DataVencimento = new DateTime(2024, 1, 10),
            EhRecorrente = true,
            TipoRecorrencia = TipoRecorrencia.Mensal
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().NotBeNull();
        proximaConta.Descricao.Should().Be("Aluguel - Janeiro");
        proximaConta.ValorOriginal.Should().Be(1000.00M);
        proximaConta.DataVencimento.Should().Be(new DateTime(2024, 2, 10));
        proximaConta.EhRecorrente.Should().BeTrue();
        proximaConta.TipoRecorrencia.Should().Be(TipoRecorrencia.Mensal);
        proximaConta.Status.Should().Be(StatusContaPagar.Pendente);
    }

    [Fact]
    public void ContaPagar_GerarProximaParcela_ContaNaoRecorrente_DeveRetornarNull()
    {
        // Arrange
        var conta = new ContaPagar
        {
            EhRecorrente = false
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoRecorrencia.Semanal, 7)]
    [InlineData(TipoRecorrencia.Quinzenal, 15)]
    public void ContaPagar_GerarProximaParcela_TiposRecorrenciaDias_DeveCalcularDataCorreta(
        TipoRecorrencia tipo, int diasEsperados)
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 10);
        var conta = new ContaPagar
        {
            DataVencimento = dataBase,
            EhRecorrente = true,
            TipoRecorrencia = tipo
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().NotBeNull();
        proximaConta!.DataVencimento.Should().Be(dataBase.AddDays(diasEsperados));
    }

    [Theory]
    [InlineData(TipoRecorrencia.Mensal, 1)]
    [InlineData(TipoRecorrencia.Bimestral, 2)]
    [InlineData(TipoRecorrencia.Trimestral, 3)]
    public void ContaPagar_GerarProximaParcela_TiposRecorrenciaMeses_DeveCalcularDataCorreta(
        TipoRecorrencia tipo, int mesesEsperados)
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 10);
        var conta = new ContaPagar
        {
            DataVencimento = dataBase,
            EhRecorrente = true,
            TipoRecorrencia = tipo
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().NotBeNull();
        proximaConta!.DataVencimento.Should().Be(dataBase.AddMonths(mesesEsperados));
    }

    [Fact]
    public void ContaPagar_GerarProximaParcela_TipoRecorrenciaAnual_DeveCalcularDataCorreta()
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 10);
        var conta = new ContaPagar
        {
            DataVencimento = dataBase,
            EhRecorrente = true,
            TipoRecorrencia = TipoRecorrencia.Anual
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().NotBeNull();
        proximaConta!.DataVencimento.Should().Be(dataBase.AddYears(1));
    }
}
