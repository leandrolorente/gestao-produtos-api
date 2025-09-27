using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.Entities;

public class ClienteTests
{
    [Fact]
    public void Cliente_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var cliente = new Cliente();

        // Assert
        cliente.Id.Should().NotBeNullOrEmpty();
        cliente.Ativo.Should().BeTrue();
        cliente.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cliente.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cliente.Nome.Should().Be(string.Empty);
        cliente.Telefone.Should().Be(string.Empty);
        cliente.Email.Should().BeNull();
        cliente.CpfCnpj.Should().BeNull();
    }

    [Fact]
    public void Cliente_WhenCreatedWithData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var nome = "Cliente Test";
        var email = "cliente@test.com";
        var telefone = "(11) 9999-9999";
        var cpfCnpj = "123.456.789-00";
        var tipo = TipoCliente.PessoaFisica;

        // Act
        var cliente = new Cliente
        {
            Nome = nome,
            Email = new Email(email),
            Telefone = telefone,
            CpfCnpj = new CpfCnpj(cpfCnpj),
            Tipo = tipo
        };

        // Assert
        cliente.Nome.Should().Be(nome);
        cliente.Email.Valor.Should().Be(email);
        cliente.Telefone.Should().Be(telefone);
        cliente.CpfCnpj.Valor.Should().Be(cpfCnpj);
        cliente.Tipo.Should().Be(tipo);
    }

    [Fact]
    public void EhPessoaFisica_WhenTipoIsPessoaFisica_ShouldReturnTrue()
    {
        // Arrange
        var cliente = new Cliente { Tipo = TipoCliente.PessoaFisica };

        // Act & Assert
        cliente.EhPessoaFisica.Should().BeTrue();
        cliente.EhPessoaJuridica.Should().BeFalse();
    }

    [Fact]
    public void EhPessoaJuridica_WhenTipoIsPessoaJuridica_ShouldReturnTrue()
    {
        // Arrange
        var cliente = new Cliente { Tipo = TipoCliente.PessoaJuridica };

        // Act & Assert
        cliente.EhPessoaJuridica.Should().BeTrue();
        cliente.EhPessoaFisica.Should().BeFalse();
    }

    [Fact]
    public void AtualizarUltimaCompra_ShouldUpdateLastPurchaseAndTimestamp()
    {
        // Arrange
        var cliente = new Cliente();
        var dataAnterior = cliente.DataAtualizacao;

        // Aguarda um pouco para garantir diferença no timestamp
        Thread.Sleep(10);

        // Act
        cliente.AtualizarUltimaCompra();

        // Assert
        cliente.UltimaCompra.Should().NotBeNull();
        cliente.UltimaCompra.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        cliente.DataAtualizacao.Should().BeAfter(dataAnterior);
    }

    [Fact]
    public void AtualizarInformacoes_ShouldUpdateClientDataAndTimestamp()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "Nome Original",
            Email = new Email("original@test.com"),
            Telefone = "(11) 1111-1111"
        };
        var dataAnterior = cliente.DataAtualizacao;

        var novoNome = "Nome Atualizado";
        var novoEmail = "novo@test.com";
        var novoTelefone = "(11) 2222-2222";

        // Aguarda um pouco para garantir diferença no timestamp
        Thread.Sleep(10);

        // Act
        cliente.AtualizarInformacoes(novoNome, novoEmail, novoTelefone);

        // Assert
        cliente.Nome.Should().Be(novoNome);
        cliente.Email.Valor.Should().Be(novoEmail);
        cliente.Telefone.Should().Be(novoTelefone);
        cliente.DataAtualizacao.Should().BeAfter(dataAnterior);
    }

    [Theory]
    [InlineData(15, 30, true)]  // Compra há 15 dias, prazo 30 dias - tem compra recente
    [InlineData(45, 30, false)] // Compra há 45 dias, prazo 30 dias - não tem compra recente
    [InlineData(29, 30, true)]  // Compra há 29 dias, prazo 30 dias - tem compra recente
    public void TemCompraRecente_ShouldReturnCorrectValue(int diasAtras, int prazoDias, bool expectedResult)
    {
        // Arrange
        var dataCompra = DateTime.UtcNow.AddDays(-diasAtras);
        var cliente = new Cliente
        {
            UltimaCompra = dataCompra
        };

        // Act
        var result = cliente.TemCompraRecente(prazoDias);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void TemCompraRecente_WhenUltimaCompraIsNull_ShouldReturnFalse()
    {
        // Arrange
        var cliente = new Cliente { UltimaCompra = null };

        // Act
        var result = cliente.TemCompraRecente();

        // Assert
        result.Should().BeFalse();
    }
}