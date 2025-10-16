using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.Entities;

public class FornecedorTests
{
    private readonly Fornecedor _fornecedor;

    public FornecedorTests()
    {
        _fornecedor = new Fornecedor
        {
            RazaoSocial = "Fornecedor Teste LTDA",
            NomeFantasia = "Fornecedor Teste",
            CnpjCpf = new CpfCnpj("12.345.678/0001-90"),
            Email = new Email("contato@fornecedorteste.com"),
            Telefone = "(11) 99999-9999",
            Tipo = TipoFornecedor.Nacional,
            Status = StatusFornecedor.Ativo,
            PrazoPagamentoPadrao = 30,
            LimiteCredito = 50000m
        };
    }

    [Fact]
    public void Fornecedor_DeveTerPropriedadesBasicasDefinidas()
    {
        // Arrange & Act
        var fornecedor = new Fornecedor
        {
            RazaoSocial = "Teste LTDA",
            NomeFantasia = "Teste",
            CnpjCpf = new CpfCnpj("12.345.678/0001-90"),
            Email = new Email("teste@teste.com"),
            Tipo = TipoFornecedor.Nacional,
            Status = StatusFornecedor.Ativo
        };

        // Assert
        fornecedor.RazaoSocial.Should().Be("Teste LTDA");
        fornecedor.NomeFantasia.Should().Be("Teste");
        fornecedor.CnpjCpf.Valor.Should().Be("12.345.678/0001-90");
        fornecedor.Email.Valor.Should().Be("teste@teste.com");
        fornecedor.Tipo.Should().Be(TipoFornecedor.Nacional);
        fornecedor.Status.Should().Be(StatusFornecedor.Ativo);
        fornecedor.ProdutoIds.Should().BeEmpty();
        fornecedor.TotalComprado.Should().Be(0);
        fornecedor.QuantidadeCompras.Should().Be(0);
    }

    [Fact]
    public void Bloquear_DeveAlterarStatusParaBloqueado()
    {
        // Arrange
        var motivo = "Inadimplência";

        // Act
        _fornecedor.Bloquear(motivo);

        // Assert
        _fornecedor.Status.Should().Be(StatusFornecedor.Bloqueado);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Desbloquear_DeveAlterarStatusParaAtivo()
    {
        // Arrange
        _fornecedor.Bloquear("Teste");

        // Act
        _fornecedor.Desbloquear();

        // Assert
        _fornecedor.Status.Should().Be(StatusFornecedor.Ativo);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Inativar_DeveAlterarStatusParaInativo()
    {
        // Act
        _fornecedor.Inativar();

        // Assert
        _fornecedor.Status.Should().Be(StatusFornecedor.Inativo);
        _fornecedor.Ativo.Should().BeFalse();
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AdicionarProduto_DeveAdicionarProdutoNaLista()
    {
        // Arrange
        var produtoId = "produto123";

        // Act
        _fornecedor.AdicionarProduto(produtoId);

        // Assert
        _fornecedor.ProdutoIds.Should().Contain(produtoId);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AdicionarProduto_NaoDeveAdicionarProdutoDuplicado()
    {
        // Arrange
        var produtoId = "produto123";
        _fornecedor.AdicionarProduto(produtoId);

        // Act
        _fornecedor.AdicionarProduto(produtoId);

        // Assert
        _fornecedor.ProdutoIds.Should().ContainSingle(p => p == produtoId);
    }

    [Fact]
    public void RemoverProduto_DeveRemoverProdutoDaLista()
    {
        // Arrange
        var produtoId = "produto123";
        _fornecedor.AdicionarProduto(produtoId);

        // Act
        _fornecedor.RemoverProduto(produtoId);

        // Assert
        _fornecedor.ProdutoIds.Should().NotContain(produtoId);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarCompra_DeveAtualizarEstatisticasDeCompra()
    {
        // Arrange
        var valor = 1000m;

        // Act
        _fornecedor.RegistrarCompra(valor);

        // Assert
        _fornecedor.TotalComprado.Should().Be(valor);
        _fornecedor.QuantidadeCompras.Should().Be(1);
        _fornecedor.UltimaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarCompra_ComMultiplasCompras_DeveAcumularValores()
    {
        // Arrange
        var valor1 = 1000m;
        var valor2 = 1500m;

        // Act
        _fornecedor.RegistrarCompra(valor1);
        _fornecedor.RegistrarCompra(valor2);

        // Assert
        _fornecedor.TotalComprado.Should().Be(valor1 + valor2);
        _fornecedor.QuantidadeCompras.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void RegistrarCompra_ComValorInvalido_DeveLancarExcecao(decimal valor)
    {
        // Act & Assert
        _fornecedor.Invoking(f => f.RegistrarCompra(valor))
            .Should().Throw<ArgumentException>()
            .WithMessage("Valor da compra deve ser maior que zero");
    }

    [Fact]
    public void CalcularTicketMedio_SemCompras_DeveRetornarZero()
    {
        // Act
        var ticketMedio = _fornecedor.CalcularTicketMedio();

        // Assert
        ticketMedio.Should().Be(0);
    }

    [Fact]
    public void CalcularTicketMedio_ComCompras_DeveCalcularCorretamente()
    {
        // Arrange
        _fornecedor.RegistrarCompra(1000m);
        _fornecedor.RegistrarCompra(2000m);
        _fornecedor.RegistrarCompra(3000m);

        // Act
        var ticketMedio = _fornecedor.CalcularTicketMedio();

        // Assert
        ticketMedio.Should().Be(2000m); // (1000 + 2000 + 3000) / 3 = 2000
    }

    [Fact]
    public void EhFrequente_ComMenosDe5Compras_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.RegistrarCompra(1000m);
        _fornecedor.RegistrarCompra(1000m);

        // Act
        var ehFrequente = _fornecedor.EhFrequente();

        // Assert
        ehFrequente.Should().BeFalse();
    }

    [Fact]
    public void EhFrequente_Com5OuMaisCompras_DeveRetornarTrue()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _fornecedor.RegistrarCompra(1000m);
        }

        // Act
        var ehFrequente = _fornecedor.EhFrequente();

        // Assert
        ehFrequente.Should().BeTrue();
    }

    [Fact]
    public void EstaComLimiteExcedido_ComTotalMenorQueLimite_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.RegistrarCompra(10000m); // Menor que limite de 50000

        // Act
        var limiteExcedido = _fornecedor.EstaComLimiteExcedido();

        // Assert
        limiteExcedido.Should().BeFalse();
    }

    [Fact]
    public void EstaComLimiteExcedido_ComTotalMaiorQueLimite_DeveRetornarTrue()
    {
        // Arrange
        _fornecedor.RegistrarCompra(60000m); // Maior que limite de 50000

        // Act
        var limiteExcedido = _fornecedor.EstaComLimiteExcedido();

        // Assert
        limiteExcedido.Should().BeTrue();
    }

    [Fact]
    public void EstaComLimiteExcedido_SemLimiteDefinido_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.LimiteCredito = 0;
        _fornecedor.RegistrarCompra(100000m);

        // Act
        var limiteExcedido = _fornecedor.EstaComLimiteExcedido();

        // Assert
        limiteExcedido.Should().BeFalse();
    }

    [Fact]
    public void PodeComprar_ComStatusAtivo_DeveRetornarTrue()
    {
        // Act
        var podeComprar = _fornecedor.PodeComprar();

        // Assert
        podeComprar.Should().BeTrue();
    }

    [Fact]
    public void PodeComprar_ComStatusBloqueado_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.Bloquear("Teste");

        // Act
        var podeComprar = _fornecedor.PodeComprar();

        // Assert
        podeComprar.Should().BeFalse();
    }

    [Fact]
    public void PodeComprar_ComStatusInativo_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.Inativar();

        // Act
        var podeComprar = _fornecedor.PodeComprar();

        // Assert
        podeComprar.Should().BeFalse();
    }

    [Fact]
    public void AtualizarDados_DeveAtualizarCamposCorretamente()
    {
        // Arrange
        var novaRazaoSocial = "Nova Razão Social";
        var novoNomeFantasia = "Novo Nome Fantasia";
        var novoTelefone = "(11) 88888-8888";
        var novoContato = "João Silva";

        // Act
        _fornecedor.AtualizarDados(novaRazaoSocial, novoNomeFantasia, novoTelefone, novoContato);

        // Assert
        _fornecedor.RazaoSocial.Should().Be(novaRazaoSocial);
        _fornecedor.NomeFantasia.Should().Be(novoNomeFantasia);
        _fornecedor.Telefone.Should().Be(novoTelefone);
        _fornecedor.ContatoPrincipal.Should().Be(novoContato);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AtualizarCondicoesComerciais_DeveAtualizarCamposCorretamente()
    {
        // Arrange
        var novoPrazo = 45;
        var novoLimite = 75000m;
        var novasCondicoes = "Pagamento em 2x";

        // Act
        _fornecedor.AtualizarCondicoesComerciais(novoPrazo, novoLimite, novasCondicoes);

        // Assert
        _fornecedor.PrazoPagamentoPadrao.Should().Be(novoPrazo);
        _fornecedor.LimiteCredito.Should().Be(novoLimite);
        _fornecedor.CondicoesPagamento.Should().Be(novasCondicoes);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AtualizarDadosBancarios_DeveAtualizarCamposCorretamente()
    {
        // Arrange
        var banco = "Banco do Brasil";
        var agencia = "1234-5";
        var conta = "12345-6";
        var pix = "fornecedor@teste.com";

        // Act
        _fornecedor.AtualizarDadosBancarios(banco, agencia, conta, pix);

        // Assert
        _fornecedor.Banco.Should().Be(banco);
        _fornecedor.Agencia.Should().Be(agencia);
        _fornecedor.Conta.Should().Be(conta);
        _fornecedor.Pix.Should().Be(pix);
        _fornecedor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TemCompraRecente_ComCompraUltimos90Dias_DeveRetornarTrue()
    {
        // Arrange
        _fornecedor.UltimaCompra = DateTime.UtcNow.AddDays(-30); // 30 dias atrás

        // Act
        var temCompraRecente = _fornecedor.TemCompraRecente(90);

        // Assert
        temCompraRecente.Should().BeTrue();
    }

    [Fact]
    public void TemCompraRecente_ComCompraMaisAntigaQuePeriodo_DeveRetornarFalse()
    {
        // Arrange
        _fornecedor.UltimaCompra = DateTime.UtcNow.AddDays(-100); // 100 dias atrás

        // Act
        var temCompraRecente = _fornecedor.TemCompraRecente(90);

        // Assert
        temCompraRecente.Should().BeFalse();
    }

    [Fact]
    public void TemCompraRecente_SemNenhumaCompra_DeveRetornarFalse()
    {
        // Act
        var temCompraRecente = _fornecedor.TemCompraRecente(90);

        // Assert
        temCompraRecente.Should().BeFalse();
    }

    [Fact]
    public void ObterResumoComercial_DeveRetornarInformacoesCorretas()
    {
        // Arrange
        _fornecedor.RegistrarCompra(2000m);
        _fornecedor.RegistrarCompra(3000m);

        // Act
        var resumo = _fornecedor.ObterResumoComercial();

        // Assert
        resumo.Should().Contain("Total comprado: R$ 5.000,00");
        resumo.Should().Contain("Quantidade de compras: 2");
        resumo.Should().Contain("Ticket médio: R$ 2.500,00");
        resumo.Should().Contain("Status: Ativo");
        resumo.Should().Contain("É frequente: Não");
    }
}
