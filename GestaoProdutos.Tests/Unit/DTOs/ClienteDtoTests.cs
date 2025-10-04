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
        var enderecoDto = new EnderecoDto
        {
            Id = "endereco1",
            Cep = "01234567",
            Logradouro = "Rua Test",
            Numero = "123",
            Bairro = "Centro",
            Localidade = "São Paulo",
            Uf = "SP",
            Estado = "São Paulo",
            Regiao = "Sudeste",
            IsPrincipal = true,
            Tipo = "Residencial",
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        var cliente = new ClienteDto
        {
            Id = "1",
            Nome = "Cliente Test",
            Email = "cliente@test.com",
            Telefone = "(11) 9999-9999",
            CpfCnpj = "123.456.789-00",
            Endereco = enderecoDto,
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
        cliente.Endereco.Should().NotBeNull();
        cliente.Endereco!.Logradouro.Should().Be("Rua Test");
        cliente.Endereco.Localidade.Should().Be("São Paulo");
        cliente.Endereco.Uf.Should().Be("SP");
        cliente.Tipo.Should().Be("Pessoa Física");
        cliente.Ativo.Should().BeTrue();
        cliente.Observacoes.Should().Be("Cliente VIP");
    }

    [Fact]
    public void CreateClienteDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var enderecoDto = new CreateEnderecoDto
        {
            Cep = "20000000",
            Logradouro = "Rua Nova",
            Numero = "456",
            Bairro = "Centro",
            Localidade = "Rio de Janeiro",
            Uf = "RJ",
            Estado = "Rio de Janeiro",
            Regiao = "Sudeste",
            IsPrincipal = true,
            Tipo = "Residencial"
        };

        var createDto = new CreateClienteDto
        {
            Nome = "Novo Cliente",
            Email = "novo@test.com",
            Telefone = "(11) 7777-7777",
            CpfCnpj = "987.654.321-00",
            Endereco = enderecoDto,
            Tipo = TipoCliente.PessoaJuridica,
            Observacoes = "Cliente empresarial"
        };

        // Assert
        createDto.Nome.Should().Be("Novo Cliente");
        createDto.Email.Should().Be("novo@test.com");
        createDto.Telefone.Should().Be("(11) 7777-7777");
        createDto.CpfCnpj.Should().Be("987.654.321-00");
        createDto.Endereco.Should().NotBeNull();
        createDto.Endereco.Logradouro.Should().Be("Rua Nova");
        createDto.Endereco.Localidade.Should().Be("Rio de Janeiro");
        createDto.Endereco.Uf.Should().Be("RJ");
        createDto.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        createDto.Observacoes.Should().Be("Cliente empresarial");
    }

    [Fact]
    public void UpdateClienteDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var enderecoDto = new UpdateEnderecoDto
        {
            Cep = "30000000",
            Logradouro = "Rua Atualizada",
            Numero = "789",
            Bairro = "Centro",
            Localidade = "Belo Horizonte",
            Uf = "MG",
            Estado = "Minas Gerais",
            Regiao = "Sudeste",
            IsPrincipal = true,
            Tipo = "Comercial"
        };

        var updateDto = new UpdateClienteDto
        {
            Nome = "Cliente Atualizado",
            Email = "atualizado@test.com",
            Telefone = "(11) 8888-8888",
            CpfCnpj = "111.222.333-44",
            Endereco = enderecoDto,
            Tipo = TipoCliente.PessoaFisica,
            Observacoes = "Cliente atualizado"
        };

        // Assert
        updateDto.Nome.Should().Be("Cliente Atualizado");
        updateDto.Email.Should().Be("atualizado@test.com");
        updateDto.Telefone.Should().Be("(11) 8888-8888");
        updateDto.CpfCnpj.Should().Be("111.222.333-44");
        updateDto.Endereco.Should().NotBeNull();
        updateDto.Endereco.Logradouro.Should().Be("Rua Atualizada");
        updateDto.Endereco.Localidade.Should().Be("Belo Horizonte");
        updateDto.Endereco.Uf.Should().Be("MG");
        updateDto.Tipo.Should().Be(TipoCliente.PessoaFisica);
        updateDto.Observacoes.Should().Be("Cliente atualizado");
    }
}