using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.ValueObjects;

public class CnpjTests
{
    [Theory]
    [InlineData("12.345.678/0001-90")]
    [InlineData("11222333000181")]
    [InlineData("12345678000195")]
    public void CpfCnpj_WithValidCnpj_ShouldCreateSuccessfully(string cnpjValue)
    {
        // Act
        var cnpj = new CpfCnpj(cnpjValue);

        // Assert
        cnpj.Valor.Should().Be(cnpjValue);
        cnpj.EhCnpj.Should().BeTrue();
        cnpj.EhCpf.Should().BeFalse();
    }

    [Fact]
    public void CpfCnpj_WithCnpj_ShouldIdentifyCorrectly()
    {
        // Arrange
        var cnpjValue = "12345678000195"; // 14 dígitos

        // Act
        var cpfCnpj = new CpfCnpj(cnpjValue);

        // Assert
        cpfCnpj.EhCnpj.Should().BeTrue();
        cpfCnpj.EhCpf.Should().BeFalse();
    }

    [Fact]
    public void CpfCnpj_WithCpf_ShouldIdentifyCorrectly()
    {
        // Arrange
        var cpfValue = "12345678901"; // 11 dígitos

        // Act
        var cpfCnpj = new CpfCnpj(cpfValue);

        // Assert
        cpfCnpj.EhCpf.Should().BeTrue();
        cpfCnpj.EhCnpj.Should().BeFalse();
    }

    [Theory]
    [InlineData("12.345.678/0001-90", "12345678000190")]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    public void CpfCnpj_WithFormattedCnpj_ShouldExtractDigitsCorrectly(string formattedCnpj, string expectedDigits)
    {
        // Act
        var cnpj = new CpfCnpj(formattedCnpj);

        // Assert
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(cnpj.Valor, @"[^\d]", "");
        digitsOnly.Should().Be(expectedDigits);
        cnpj.EhCnpj.Should().BeTrue();
    }

    [Fact]
    public void CpfCnpj_ImplicitConversionFromString_ShouldWork()
    {
        // Arrange
        string cnpjValue = "12345678000195";

        // Act
        CpfCnpj cnpj = cnpjValue;

        // Assert
        cnpj.Valor.Should().Be(cnpjValue);
        cnpj.EhCnpj.Should().BeTrue();
    }

    [Fact]
    public void CpfCnpj_ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        var cnpjValue = "12345678000195";
        var cnpj = new CpfCnpj(cnpjValue);

        // Act
        string convertedValue = cnpj;

        // Assert
        convertedValue.Should().Be(cnpjValue);
    }
}
