using Xunit;
using FluentAssertions;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("firstname+lastname@company.org")]
    [InlineData("email@subdomain.example.com")]
    public void Email_WithValidFormat_ShouldCreateSuccessfully(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        email.Valor.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Email_WithEmptyOrWhitespace_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(invalidEmail!));
        exception.Message.Should().Contain("Email não pode ser vazio");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    [InlineData("user name@domain.com")]
    [InlineData("user@domain .com")]
    public void Email_WithInvalidFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
        exception.Message.Should().Contain("Email inválido");
    }

    [Fact]
    public void Email_ShouldConvertToLowerCase()
    {
        // Arrange
        var upperCaseEmail = "TEST@EXAMPLE.COM";

        // Act
        var email = new Email(upperCaseEmail);

        // Assert
        email.Valor.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_ImplicitConversionToString_ShouldWork()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = new Email(emailValue);

        // Act
        string convertedEmail = email;

        // Assert
        convertedEmail.Should().Be(emailValue);
    }

    [Fact]
    public void Email_ImplicitConversionFromString_ShouldWork()
    {
        // Arrange
        var emailValue = "test@example.com";

        // Act
        Email email = emailValue;

        // Assert
        email.Valor.Should().Be(emailValue);
    }
}