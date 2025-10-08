using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.ValueObjects;

public class EnderecoTests
{
    [Fact]
    public void Endereco_WhenCreatedWithAllProperties_ShouldSetCorrectly()
    {
        // Arrange
        var logradouro = "Rua das Flores, 123";
        var cidade = "São Paulo";
        var estado = "SP";
        var cep = "01234-567";
        var complemento = "Apto 45";

        // Act
        var endereco = new Endereco
        {
            Logradouro = logradouro,
            Cidade = cidade,
            Estado = estado,
            Cep = cep,
            Complemento = complemento
        };

        // Assert
        endereco.Logradouro.Should().Be(logradouro);
        endereco.Cidade.Should().Be(cidade);
        endereco.Estado.Should().Be(estado);
        endereco.Cep.Should().Be(cep);
        endereco.Complemento.Should().Be(complemento);
    }

    [Fact]
    public void EnderecoCompleto_ShouldFormatCorrectly()
    {
        // Arrange
        var endereco = new Endereco
        {
            Logradouro = "Rua das Flores, 123",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01234-567"
        };

        // Act
        var enderecoCompleto = endereco.EnderecoCompleto;

        // Assert
        enderecoCompleto.Should().Be("Rua das Flores, 123, São Paulo/SP - 01234-567");
    }

    [Fact]
    public void Endereco_DefaultConstructor_ShouldHaveEmptyStrings()
    {
        // Act
        var endereco = new Endereco();

        // Assert
        endereco.Logradouro.Should().Be(string.Empty);
        endereco.Cidade.Should().Be(string.Empty);
        endereco.Estado.Should().Be(string.Empty);
        endereco.Cep.Should().Be(string.Empty);
        endereco.Complemento.Should().BeNull();
    }

    [Theory]
    [InlineData("Rua A", "Cidade A", "AA", "11111-111", null, "Rua A, Cidade A/AA - 11111-111")]
    [InlineData("Av. B", "Cidade B", "BB", "22222-222", "Bloco 1", "Av. B, Cidade B/BB - 22222-222")]
    [InlineData("", "", "", "", null, ", / - ")]
    public void EnderecoCompleto_WithDifferentValues_ShouldFormatCorrectly(
        string logradouro, string cidade, string estado, string cep, string? complemento, string expectedResult)
    {
        // Arrange
        var endereco = new Endereco
        {
            Logradouro = logradouro,
            Cidade = cidade,
            Estado = estado,
            Cep = cep,
            Complemento = complemento
        };

        // Act
        var result = endereco.EnderecoCompleto;

        // Assert
        result.Should().Be(expectedResult);
    }
}
