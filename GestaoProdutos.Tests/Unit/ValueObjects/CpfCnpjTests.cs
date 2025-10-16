using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.ValueObjects;

public class CpfCnpjTests
{
    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("12345678900")]
    [InlineData("987.654.321-11")]
    public void CpfCnpj_WithValidCpf_ShouldCreateSuccessfully(string validCpf)
    {
        // Act
        var cpfCnpj = new CpfCnpj(validCpf);

        // Assert
        cpfCnpj.Valor.Should().Be(validCpf);
        cpfCnpj.EhCpf.Should().BeTrue();
        cpfCnpj.EhCnpj.Should().BeFalse();
    }

    [Theory]
    [InlineData("12.345.678/0001-90")]
    [InlineData("12345678000190")]
    [InlineData("98.765.432/0001-10")]
    public void CpfCnpj_WithValidCnpj_ShouldCreateSuccessfully(string validCnpj)
    {
        // Act
        var cpfCnpj = new CpfCnpj(validCnpj);

        // Assert
        cpfCnpj.Valor.Should().Be(validCnpj);
        cpfCnpj.EhCnpj.Should().BeTrue();
        cpfCnpj.EhCpf.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CpfCnpj_WithEmptyOrWhitespace_ShouldThrowArgumentException(string? invalidCpfCnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CpfCnpj(invalidCpfCnpj!));
        exception.Message.Should().Contain("CPF/CNPJ não pode ser vazio");
    }

    [Theory]
    [InlineData("123")]           // Muito curto
    [InlineData("123456789")]     // 9 dígitos
    [InlineData("123456789012")]  // 12 dígitos
    [InlineData("12345678901234567")] // 17 dígitos
    [InlineData("123456789012345")] // 15 dígitos
    public void CpfCnpj_WithInvalidLength_ShouldThrowArgumentException(string invalidCpfCnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CpfCnpj(invalidCpfCnpj));
        exception.Message.Should().Contain("CPF/CNPJ deve ter 11 ou 14 dígitos");
    }

    [Fact]
    public void CpfCnpj_ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        var cpfValue = "123.456.789-00";
        var cpfCnpj = new CpfCnpj(cpfValue);

        // Act
        string convertedValue = cpfCnpj;

        // Assert
        convertedValue.Should().Be(cpfValue);
    }

    [Fact]
    public void CpfCnpj_ImplicitConversionFromString_ShouldWork()
    {
        // Arrange
        var cpfValue = "123.456.789-00";

        // Act
        CpfCnpj cpfCnpj = cpfValue;

        // Assert
        cpfCnpj.Valor.Should().Be(cpfValue);
    }
}
