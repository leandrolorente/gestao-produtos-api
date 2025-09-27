using Xunit;
using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Tests.Unit.DTOs;

public class ProdutoDtoTests
{
    [Fact]
    public void ProdutoDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var produto = new ProdutoDto
        {
            Id = "1",
            Name = "Produto Test",
            Sku = "TEST001",
            Quantity = 100,
            Price = 50.99m,
            LastUpdated = DateTime.UtcNow,
            Categoria = "Eletrônicos",
            EstoqueBaixo = false
        };

        // Assert
        produto.Id.Should().Be("1");
        produto.Name.Should().Be("Produto Test");
        produto.Sku.Should().Be("TEST001");
        produto.Quantity.Should().Be(100);
        produto.Price.Should().Be(50.99m);
        produto.Categoria.Should().Be("Eletrônicos");
        produto.EstoqueBaixo.Should().BeFalse();
    }

    [Fact]
    public void CreateProdutoDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var createDto = new CreateProdutoDto
        {
            Name = "Novo Produto",
            Sku = "NEW001",
            Quantity = 50,
            Price = 29.99m,
            Categoria = "Categoria Test",
            Descricao = "Descrição test",
            PrecoCompra = 20.00m,
            EstoqueMinimo = 10
        };

        // Assert
        createDto.Name.Should().Be("Novo Produto");
        createDto.Sku.Should().Be("NEW001");
        createDto.Quantity.Should().Be(50);
        createDto.Price.Should().Be(29.99m);
        createDto.Categoria.Should().Be("Categoria Test");
        createDto.Descricao.Should().Be("Descrição test");
        createDto.PrecoCompra.Should().Be(20.00m);
        createDto.EstoqueMinimo.Should().Be(10);
    }

    [Fact]
    public void UpdateProdutoDto_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var updateDto = new UpdateProdutoDto
        {
            Name = "Produto Atualizado",
            Quantity = 75,
            Price = 39.99m,
            Categoria = "Nova Categoria",
            Descricao = "Nova descrição",
            PrecoCompra = 25.00m,
            EstoqueMinimo = 15
        };

        // Assert
        updateDto.Name.Should().Be("Produto Atualizado");
        updateDto.Quantity.Should().Be(75);
        updateDto.Price.Should().Be(39.99m);
        updateDto.Categoria.Should().Be("Nova Categoria");
        updateDto.Descricao.Should().Be("Nova descrição");
        updateDto.PrecoCompra.Should().Be(25.00m);
        updateDto.EstoqueMinimo.Should().Be(15);
    }
}