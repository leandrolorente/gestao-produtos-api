using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GestaoProdutos.Tests.Unit.Services;

public class AuthServiceSimpleTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _authService;

    public AuthServiceSimpleTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        _mockConfiguration.Setup(x => x["JWT:Secret"]).Returns("MinhaChaveSecretaParaTestesComMaisDe32Caracteres123456");
        _mockConfiguration.Setup(x => x["JWT:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(x => x["JWT:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(x => x["JWT:ExpirationHours"]).Returns("24");

        _mockUnitOfWork.Setup(x => x.Usuarios).Returns(_mockUsuarioRepository.Object);

        _authService = new AuthService(_mockUnitOfWork.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ValidUserId_ShouldReturnUserDto()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";
        var usuario = new Usuario
        {
            Id = userId,
            Nome = "Test User",
            Role = UserRole.User,
            Avatar = "avatar.jpg",
            Departamento = "IT",
            Ativo = true,
            UltimoLogin = DateTime.UtcNow.AddDays(-1),
            DataCriacao = DateTime.UtcNow.AddDays(-30),
            DataAtualizacao = DateTime.UtcNow
        };
        usuario.Email = new Email("test@test.com");

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(usuario);

        // Act
        var result = await _authService.GetCurrentUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@test.com");
        result.Role.Should().Be("user");
        result.Avatar.Should().Be("avatar.jpg");
        result.Department.Should().Be("IT");
        result.IsActive.Should().BeTrue();
        result.LastLogin.Should().Be(usuario.UltimoLogin);
        result.LastUpdated.Should().Be(usuario.DataAtualizacao);
    }

    [Fact]
    public async Task GetCurrentUserAsync_NonExistentUser_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((Usuario?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.GetCurrentUserAsync(userId));

        exception.Message.Should().Be("Usuário não encontrado");
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Usuario>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(loginDto));

        exception.Message.Should().Be("Credenciais inválidas");
    }

    [Fact]
    public async Task ValidateTokenAsync_EmptyToken_ShouldReturnFalse()
    {
        // Arrange
        var emptyToken = "";

        // Act
        var result = await _authService.ValidateTokenAsync(emptyToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ForgotPasswordAsync_NonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "nonexistent@test.com"
        };

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Usuario>());

        // Act
        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_NonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var resetPasswordDto = new ResetPasswordDto
        {
            Email = "nonexistent@test.com",
            Token = "validToken",
            NewPassword = "newPassword123"
        };

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Usuario>());

        // Act
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "currentPassword",
            NewPassword = "newPassword123"
        };

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

        // Assert
        result.Should().BeFalse();
    }
}