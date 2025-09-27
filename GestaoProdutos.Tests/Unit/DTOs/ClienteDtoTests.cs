using Xunit;
using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Tests.Unit.DTOs;

public class ClienteDtoTests
{
    [Fact]
    public void ClienteDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var cliente = new ClienteDto
        {
            Id = "1",
            Nome = "Cliente Test",
            Email = "cliente@test.com",
            Telefone = "(11) 9999-9999",
            CpfCnpj = "123.456.789-00",
            Endereco = "Rua Test, 123",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01234-567",
            Tipo = "Pessoa Física",
            Ativo = true,
            DataCadastro = DateTime.UtcNow,
            UltimaCompra = DateTime.UtcNow.AddDays(-10),
            Observacoes = "Cliente VIP"
        };

        // Assert
        cliente.Id.Should().Be("1");
        cliente.Nome.Should().Be("Cliente Test");
        cliente.Email.Should().Be("cliente@test.com");
        cliente.Telefone.Should().Be("(11) 9999-9999");
        cliente.CpfCnpj.Should().Be("123.456.789-00");
        cliente.Endereco.Should().Be("Rua Test, 123");
        cliente.Cidade.Should().Be("São Paulo");
        cliente.Estado.Should().Be("SP");
        cliente.Cep.Should().Be("01234-567");
        cliente.Tipo.Should().Be("Pessoa Física");
        cliente.Ativo.Should().BeTrue();
        cliente.Observacoes.Should().Be("Cliente VIP");
    }

    [Fact]
    public void CreateClienteDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var createDto = new CreateClienteDto
        {
            Nome = "Novo Cliente",
            Email = "novo@test.com",
            Telefone = "(11) 7777-7777",
            CpfCnpj = "987.654.321-00",
            Endereco = "Rua Nova, 456",
            Cidade = "Rio de Janeiro",
            Estado = "RJ",
            Cep = "20000-000",
            Tipo = TipoCliente.PessoaJuridica,
            Observacoes = "Cliente empresarial"
        };

        // Assert
        createDto.Nome.Should().Be("Novo Cliente");
        createDto.Email.Should().Be("novo@test.com");
        createDto.Telefone.Should().Be("(11) 7777-7777");
        createDto.CpfCnpj.Should().Be("987.654.321-00");
        createDto.Endereco.Should().Be("Rua Nova, 456");
        createDto.Cidade.Should().Be("Rio de Janeiro");
        createDto.Estado.Should().Be("RJ");
        createDto.Cep.Should().Be("20000-000");
        createDto.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        createDto.Observacoes.Should().Be("Cliente empresarial");
    }

    [Fact]
    public void UpdateClienteDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var updateDto = new UpdateClienteDto
        {
            Nome = "Cliente Atualizado",
            Email = "atualizado@test.com",
            Telefone = "(11) 8888-8888",
            Endereco = "Rua Atualizada, 789",
            Cidade = "Belo Horizonte",
            Estado = "MG",
            Cep = "30000-000",
            Observacoes = "Informações atualizadas"
        };

        // Assert
        updateDto.Nome.Should().Be("Cliente Atualizado");
        updateDto.Email.Should().Be("atualizado@test.com");
        updateDto.Telefone.Should().Be("(11) 8888-8888");
        updateDto.Endereco.Should().Be("Rua Atualizada, 789");
        updateDto.Cidade.Should().Be("Belo Horizonte");
        updateDto.Estado.Should().Be("MG");
        updateDto.Cep.Should().Be("30000-000");
        updateDto.Observacoes.Should().Be("Informações atualizadas");
    }
}