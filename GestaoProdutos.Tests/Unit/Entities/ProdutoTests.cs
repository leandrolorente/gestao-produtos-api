using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Tests.Unit.Entities;

public class ProdutoTests
{
    [Fact]
    public void Produto_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var produto = new Produto();

        // Assert
        produto.Id.Should().NotBeNullOrEmpty();
        produto.Ativo.Should().BeTrue();
        produto.Status.Should().Be(StatusProduto.Ativo);
        produto.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        produto.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        produto.UltimaAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Produto_WhenCreatedWithData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var nome = "Produto Test";
        var sku = "TEST001";
        var preco = 100.50m;
        var quantidade = 50;
        var descricao = "Descrição do produto test";
        var categoria = "Categoria Test";
        var estoqueMinimo = 10;

        // Act
        var produto = new Produto
        {
            Nome = nome,
            Sku = sku,
            Preco = preco,
            Quantidade = quantidade,
            Descricao = descricao,
            Categoria = categoria,
            EstoqueMinimo = estoqueMinimo
        };

        // Assert
        produto.Nome.Should().Be(nome);
        produto.Sku.Should().Be(sku);
        produto.Preco.Should().Be(preco);
        produto.Quantidade.Should().Be(quantidade);
        produto.Descricao.Should().Be(descricao);
        produto.Categoria.Should().Be(categoria);
        produto.EstoqueMinimo.Should().Be(estoqueMinimo);
    }

    [Theory]
    [InlineData(10, 5, true)]  // Estoque atual menor que mínimo
    [InlineData(10, 10, true)] // Estoque atual igual ao mínimo
    [InlineData(10, 15, false)] // Estoque atual maior que mínimo
    [InlineData(null, 5, false)] // Sem estoque mínimo definido
    public void EstaComEstoqueBaixo_ShouldReturnCorrectValue(int? estoqueMinimo, int quantidade, bool expectedResult)
    {
        // Arrange
        var produto = new Produto
        {
            Quantidade = quantidade,
            EstoqueMinimo = estoqueMinimo
        };

        // Act
        var result = produto.EstaComEstoqueBaixo();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void AtualizarEstoque_ShouldUpdateQuantityAndTimestamp()
    {
        // Arrange
        var produto = new Produto { Quantidade = 50 };
        var dataAnterior = produto.UltimaAtualizacao;
        var novaQuantidade = 75;

        // Aguarda um pouco para garantir diferença no timestamp
        Thread.Sleep(10);

        // Act
        produto.AtualizarEstoque(novaQuantidade);

        // Assert
        produto.Quantidade.Should().Be(novaQuantidade);
        produto.UltimaAtualizacao.Should().BeAfter(dataAnterior);
        produto.DataAtualizacao.Should().BeAfter(dataAnterior);
    }

    [Fact]
    public void AtualizarPreco_WithValidPrice_ShouldUpdatePriceAndTimestamp()
    {
        // Arrange
        var produto = new Produto { Preco = 100.00m };
        var dataAnterior = produto.UltimaAtualizacao;
        var novoPreco = 150.00m;

        // Aguarda um pouco para garantir diferença no timestamp
        Thread.Sleep(10);

        // Act
        produto.AtualizarPreco(novoPreco);

        // Assert
        produto.Preco.Should().Be(novoPreco);
        produto.UltimaAtualizacao.Should().BeAfter(dataAnterior);
        produto.DataAtualizacao.Should().BeAfter(dataAnterior);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-0.01)]
    public void AtualizarPreco_WithInvalidPrice_ShouldThrowArgumentException(decimal precoInvalido)
    {
        // Arrange
        var produto = new Produto { Preco = 100.00m };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => produto.AtualizarPreco(precoInvalido));
        exception.Message.Should().Contain("Preço deve ser maior que zero");
    }
}