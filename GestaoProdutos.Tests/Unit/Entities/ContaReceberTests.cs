using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using MongoDB.Bson;
using Xunit;

namespace GestaoProdutos.Tests.Unit.Entities;

/// <summary>
/// Testes unitários para a entidade ContaReceber
/// </summary>
public class ContaReceberTests
{
    [Fact]
    public void ContaReceber_CriarNova_DeveDefinirValoresIniciais()
    {
        // Arrange & Act
        var conta = new ContaReceber
        {
            Descricao = "Venda para Cliente ABC - Nota 123",
            ClienteId = ObjectId.GenerateNewId().ToString(),
            ClienteNome = "Cliente ABC",
            ValorOriginal = 1500.00M,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Status = StatusContaReceber.Pendente,
            VendaId = ObjectId.GenerateNewId().ToString()
        };

        // Assert
        conta.Descricao.Should().Be("Venda para Cliente ABC - Nota 123");
        conta.ClienteNome.Should().Be("Cliente ABC");
        conta.ValorOriginal.Should().Be(1500.00M);
        conta.Status.Should().Be(StatusContaReceber.Pendente);
        conta.ValorRecebido.Should().Be(0);
        conta.ValorRestante.Should().Be(1500.00M);
        conta.Juros.Should().Be(0);
        conta.Multa.Should().Be(0);
        conta.Desconto.Should().Be(0);
    }

    [Fact]
    public void ContaReceber_Receber_ValorTotal_DeveMarcarComoRecebida()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30)
        };

        // Act
        conta.Receber(1500.00M, FormaPagamento.CartaoCredito, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Recebida);
        conta.ValorRecebido.Should().Be(1500.00M);
        conta.ValorRestante.Should().Be(0);
        conta.DataRecebimento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        conta.FormaPagamento.Should().Be(FormaPagamento.CartaoCredito);
    }

    [Fact]
    public void ContaReceber_Receber_ValorParcial_DeveMarcarComoRecebimentoParcial()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30)
        };

        // Act
        conta.Receber(500.00M, FormaPagamento.PIX, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaReceber.RecebimentoParcial);
        conta.ValorRecebido.Should().Be(500.00M);
        conta.ValorRestante.Should().Be(1000.00M);
    }

    [Fact]
    public void ContaReceber_Receber_ContaJaRecebida_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Recebida
        };

        // Act & Assert
        var act = () => conta.Receber(100.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível receber uma conta já recebida");
    }

    [Fact]
    public void ContaReceber_Receber_ContaCancelada_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Cancelada
        };

        // Act & Assert
        var act = () => conta.Receber(100.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível receber uma conta cancelada");
    }

    [Fact]
    public void ContaReceber_Receber_ValorMaiorQueRestante_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            ValorRecebido = 1000.00M,
            Status = StatusContaReceber.RecebimentoParcial
        };

        // Act & Assert
        var act = () => conta.Receber(600.00M, FormaPagamento.PIX, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Valor do recebimento não pode ser maior que o valor restante");
    }

    [Fact]
    public void ContaReceber_Cancelar_ContaPendente_DeveCancelar()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Pendente
        };

        // Act
        conta.Cancelar();

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Cancelada);
    }

    [Fact]
    public void ContaReceber_Cancelar_ContaJaRecebida_DeveLancarExcecao()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Recebida
        };

        // Act & Assert
        var act = () => conta.Cancelar();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Não é possível cancelar uma conta já recebida");
    }

    [Theory]
    [InlineData(StatusContaReceber.Pendente, false)]
    [InlineData(StatusContaReceber.Vencida, true)]
    [InlineData(StatusContaReceber.RecebimentoParcial, false)]
    [InlineData(StatusContaReceber.Recebida, false)]
    [InlineData(StatusContaReceber.Cancelada, false)]
    public void ContaReceber_EstaVencida_DeveRetornarStatusCorreto(StatusContaReceber status, bool esperado)
    {
        // Arrange
        var conta = new ContaReceber
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
    public void ContaReceber_CalcularJuros_ContaVencida_DeveCalcularJuros()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 2000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(-15), // 15 dias em atraso
            Status = StatusContaReceber.Vencida
        };

        // Act
        var juros = conta.CalcularJuros();

        // Assert
        juros.Should().BeGreaterThan(0); // Deveria ter juros para conta vencida
    }

    [Fact]
    public void ContaReceber_CalcularJuros_ContaNaoVencida_DeveRetornarZero()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 2000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(10),
            Status = StatusContaReceber.Pendente
        };

        // Act
        var juros = conta.CalcularJuros();

        // Assert
        juros.Should().Be(0);
    }

    [Fact]
    public void ContaReceber_AtualizarStatus_ContaVencida_DeveMarcarComoVencida()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 2000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(-1), // Vencida
            Status = StatusContaReceber.Pendente
        };

        // Act
        conta.AtualizarStatus();

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Vencida);
    }

    [Fact]
    public void ContaReceber_AtualizarStatus_ContaNaoVencida_DeveManterPendente()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 2000.00M,
            DataVencimento = DateTime.UtcNow.AddDays(10),
            Status = StatusContaReceber.Pendente
        };

        // Act
        conta.AtualizarStatus();

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Pendente);
    }

    [Fact]
    public void ContaReceber_GerarProximaParcela_ContaRecorrente_DeveGerarNovaConta()
    {
        // Arrange
        var conta = new ContaReceber
        {
            Descricao = "Mensalidade - Janeiro",
            ValorOriginal = 500.00M,
            DataVencimento = new DateTime(2024, 1, 15),
            EhRecorrente = true,
            TipoRecorrencia = TipoRecorrencia.Mensal
        };

        // Act
        var proximaConta = conta.GerarProximaParcela();

        // Assert
        proximaConta.Should().NotBeNull();
        proximaConta.Descricao.Should().Be("Mensalidade - Janeiro");
        proximaConta.ValorOriginal.Should().Be(500.00M);
        proximaConta.DataVencimento.Should().Be(new DateTime(2024, 2, 15));
        proximaConta.EhRecorrente.Should().BeTrue();
        proximaConta.TipoRecorrencia.Should().Be(TipoRecorrencia.Mensal);
        proximaConta.Status.Should().Be(StatusContaReceber.Pendente);
    }

    [Fact]
    public void ContaReceber_GerarProximaParcela_ContaNaoRecorrente_DeveRetornarNull()
    {
        // Arrange
        var conta = new ContaReceber
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
    public void ContaReceber_GerarProximaParcela_TiposRecorrenciaDias_DeveCalcularDataCorreta(
        TipoRecorrencia tipo, int diasEsperados)
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 15);
        var conta = new ContaReceber
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
    public void ContaReceber_GerarProximaParcela_TiposRecorrenciaMeses_DeveCalcularDataCorreta(
        TipoRecorrencia tipo, int mesesEsperados)
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 15);
        var conta = new ContaReceber
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
    public void ContaReceber_GerarProximaParcela_TipoRecorrenciaAnual_DeveCalcularDataCorreta()
    {
        // Arrange
        var dataBase = new DateTime(2024, 1, 15);
        var conta = new ContaReceber
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

    [Fact]
    public void ContaReceber_RecebimentosMultiplos_DeveAcumularValores()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1500.00M,
            Status = StatusContaReceber.Pendente
        };

        // Act
        conta.Receber(500.00M, FormaPagamento.PIX, DateTime.UtcNow); // Primeiro recebimento
        conta.Receber(300.00M, FormaPagamento.CartaoCredito, DateTime.UtcNow); // Segundo recebimento
        conta.Receber(700.00M, FormaPagamento.Dinheiro, DateTime.UtcNow); // Recebimento final

        // Assert
        conta.ValorRecebido.Should().Be(1500.00M);
        conta.ValorRestante.Should().Be(0);
        conta.Status.Should().Be(StatusContaReceber.Recebida);
    }

    [Fact]
    public void ContaReceber_ComDesconto_DeveCalcularValorCorretamente()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1000.00M,
            Desconto = 100.00M, // 10% de desconto
            Status = StatusContaReceber.Pendente
        };

        // Act
        var valorFinal = conta.ValorOriginal - conta.Desconto;
        conta.Receber(valorFinal, FormaPagamento.PIX, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Recebida);
        conta.ValorRecebido.Should().Be(900.00M);
        conta.ValorRestante.Should().Be(0);
    }

    [Fact]
    public void ContaReceber_ComJurosEMulta_DeveCalcularValorCorretamente()
    {
        // Arrange
        var conta = new ContaReceber
        {
            ValorOriginal = 1000.00M,
            Juros = 50.00M,
            Multa = 20.00M,
            Status = StatusContaReceber.Vencida
        };

        // Act
        var valorFinal = conta.ValorOriginal + conta.Juros + conta.Multa;
        conta.Receber(valorFinal, FormaPagamento.PIX, DateTime.UtcNow);

        // Assert
        conta.Status.Should().Be(StatusContaReceber.Recebida);
        conta.ValorRecebido.Should().Be(1070.00M);
        conta.ValorRestante.Should().Be(0);
    }
}
