using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.Entities;

public class ClienteUpdateTests
{
    [Fact]
    public void AtualizarInformacoes_WhenChangingFromCpfToCnpj_ShouldUpdateTipoToPessoaJuridica()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = new Email("joao@teste.com"),
            Telefone = "(11) 99999-9999",
            CpfCnpj = new CpfCnpj("12345678901"), // CPF (11 dígitos)
            Tipo = TipoCliente.PessoaFisica
        };

        // Assert inicial
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.CpfCnpj.EhCpf.Should().BeTrue();

        // Act - Mudar para CNPJ
        cliente.AtualizarInformacoes(
            nome: "Empresa Silva LTDA",
            email: "empresa@silva.com.br",
            telefone: "(11) 3333-3333",
            cpfCnpj: "12345678000195" // CNPJ (14 dígitos)
        );

        // Assert
        cliente.Nome.Should().Be("Empresa Silva LTDA");
        cliente.Email!.Valor.Should().Be("empresa@silva.com.br");
        cliente.Telefone.Should().Be("(11) 3333-3333");
        cliente.CpfCnpj!.Valor.Should().Be("12345678000195");
        cliente.CpfCnpj.EhCnpj.Should().BeTrue();
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.EhPessoaJuridica.Should().BeTrue();
        cliente.EhPessoaFisica.Should().BeFalse();
    }

    [Fact]
    public void AtualizarInformacoes_WhenChangingFromCnpjToCpf_ShouldUpdateTipoToPessoaFisica()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "Empresa ABC LTDA",
            Email = new Email("contato@abc.com.br"),
            Telefone = "(11) 3333-3333",
            CpfCnpj = new CpfCnpj("12345678000195"), // CNPJ (14 dígitos)
            Tipo = TipoCliente.PessoaJuridica
        };

        // Assert inicial
        cliente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        cliente.CpfCnpj.EhCnpj.Should().BeTrue();

        // Act - Mudar para CPF
        cliente.AtualizarInformacoes(
            nome: "Maria Silva",
            email: "maria@teste.com",
            telefone: "(11) 99999-9999",
            cpfCnpj: "12345678901" // CPF (11 dígitos)
        );

        // Assert
        cliente.Nome.Should().Be("Maria Silva");
        cliente.Email!.Valor.Should().Be("maria@teste.com");
        cliente.Telefone.Should().Be("(11) 99999-9999");
        cliente.CpfCnpj!.Valor.Should().Be("12345678901");
        cliente.CpfCnpj.EhCpf.Should().BeTrue();
        cliente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        cliente.EhPessoaFisica.Should().BeTrue();
        cliente.EhPessoaJuridica.Should().BeFalse();
    }

    [Fact]
    public void AtualizarInformacoes_WithEmptyCpfCnpj_ShouldSetToNullAndNotChangeTipo()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = new Email("joao@teste.com"),
            CpfCnpj = new CpfCnpj("12345678901"),
            Tipo = TipoCliente.PessoaFisica
        };

        var tipoOriginal = cliente.Tipo;

        // Act
        cliente.AtualizarInformacoes(
            nome: "João Silva Atualizado",
            email: "joao.novo@teste.com",
            telefone: "(11) 88888-8888",
            cpfCnpj: "" // CPF/CNPJ vazio
        );

        // Assert
        cliente.Nome.Should().Be("João Silva Atualizado");
        cliente.Email!.Valor.Should().Be("joao.novo@teste.com");
        cliente.Telefone.Should().Be("(11) 88888-8888");
        cliente.CpfCnpj.Should().BeNull();
        cliente.Tipo.Should().Be(tipoOriginal); // Não deve mudar o tipo quando CPF/CNPJ é nulo
    }

    [Fact]
    public void AtualizarInformacoes_WithEmptyEmail_ShouldSetEmailToNull()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            Email = new Email("joao@teste.com"),
            CpfCnpj = new CpfCnpj("12345678901"),
            Tipo = TipoCliente.PessoaFisica
        };

        // Act
        cliente.AtualizarInformacoes(
            nome: "João Silva",
            email: "", // Email vazio
            telefone: "(11) 99999-9999",
            cpfCnpj: "12345678901"
        );

        // Assert
        cliente.Email.Should().BeNull();
        cliente.CpfCnpj.Should().NotBeNull();
        cliente.CpfCnpj!.Valor.Should().Be("12345678901");
    }

    [Fact]
    public void AtualizarInformacoes_ShouldUpdateDataAtualizacao()
    {
        // Arrange
        var cliente = new Cliente();
        var dataAnterior = cliente.DataAtualizacao;
        
        // Esperar um pouco para garantir diferença no timestamp
        Thread.Sleep(10);

        // Act
        cliente.AtualizarInformacoes(
            nome: "Teste",
            email: "teste@teste.com",
            telefone: "11999999999",
            cpfCnpj: "12345678901"
        );

        // Assert
        cliente.DataAtualizacao.Should().BeAfter(dataAnterior);
        cliente.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("12345678901", TipoCliente.PessoaFisica)]  // CPF
    [InlineData("12345678000195", TipoCliente.PessoaJuridica)]  // CNPJ
    [InlineData("123.456.789-01", TipoCliente.PessoaFisica)]  // CPF formatado
    [InlineData("12.345.678/0001-95", TipoCliente.PessoaJuridica)]  // CNPJ formatado
    public void AtualizarInformacoes_ShouldSetCorrectTipoBasedOnCpfCnpj(string cpfCnpjValue, TipoCliente expectedTipo)
    {
        // Arrange
        var cliente = new Cliente
        {
            Tipo = TipoCliente.PessoaFisica // Tipo inicial diferente
        };

        // Act
        cliente.AtualizarInformacoes(
            nome: "Teste Cliente",
            email: "teste@cliente.com",
            telefone: "11999999999",
            cpfCnpj: cpfCnpjValue
        );

        // Assert
        cliente.Tipo.Should().Be(expectedTipo);
        cliente.CpfCnpj.Should().NotBeNull();
        cliente.CpfCnpj!.Valor.Should().Be(cpfCnpjValue);
    }
}