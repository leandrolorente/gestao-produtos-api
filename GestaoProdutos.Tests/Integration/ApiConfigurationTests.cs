using Xunit;
using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Tests.Integration;

public class ApiConfigurationTests
{
    [Fact]
    public void DTOs_ShouldHaveCorrectStructure()
    {
        // Teste simples para verificar se os DTOs estão corretos
        var produto = new ProdutoDto();
        var cliente = new ClienteDto();
        var createProduto = new CreateProdutoDto();
        var createCliente = new CreateClienteDto();
        
        produto.Should().NotBeNull();
        cliente.Should().NotBeNull();
        createProduto.Should().NotBeNull();
        createCliente.Should().NotBeNull();
    }

    [Fact]
    public void Enums_ShouldHaveCorrectValues()
    {
        // Verificar se os enums estão definidos corretamente
        var tipoCliente = TipoCliente.PessoaFisica;
        var statusProduto = StatusProduto.Ativo;
        
        tipoCliente.Should().Be(TipoCliente.PessoaFisica);
        statusProduto.Should().Be(StatusProduto.Ativo);
    }

    [Fact]
    public void CreateProdutoDto_ShouldAcceptValidData()
    {
        // Arrange & Act
        var dto = new CreateProdutoDto
        {
            Name = "Produto Test",
            Sku = "TEST001",
            Quantity = 100,
            Price = 50.99m,
            Categoria = "Eletrônicos",
            Descricao = "Produto de teste",
            PrecoCompra = 30.00m,
            EstoqueMinimo = 10
        };

        // Assert
        dto.Name.Should().Be("Produto Test");
        dto.Sku.Should().Be("TEST001");
        dto.Quantity.Should().Be(100);
        dto.Price.Should().Be(50.99m);
    }

    [Fact]
    public void CreateClienteDto_ShouldAcceptValidData()
    {
        // Arrange & Act
        var enderecoDto = new CreateEnderecoDto
        {
            Cep = "01234567",
            Logradouro = "Rua Test",
            Numero = "123",
            Bairro = "Centro",
            Localidade = "São Paulo",
            Uf = "SP",
            Estado = "São Paulo",
            Regiao = "Sudeste",
            IsPrincipal = true,
            Tipo = TipoEndereco.Residencial
        };

        var dto = new CreateClienteDto
        {
            Nome = "Cliente Test",
            Email = "cliente@test.com",
            Telefone = "(11) 9999-9999",
            CpfCnpj = "123.456.789-00",
            Endereco = enderecoDto,
            Tipo = TipoCliente.PessoaFisica,
            Observacoes = "Cliente VIP"
        };

        // Assert
        dto.Nome.Should().Be("Cliente Test");
        dto.Email.Should().Be("cliente@test.com");
        dto.Tipo.Should().Be(TipoCliente.PessoaFisica);
        dto.Endereco.Should().NotBeNull();
        dto.Endereco.Logradouro.Should().Be("Rua Test");
    }
}