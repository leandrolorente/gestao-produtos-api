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

public class UserServiceSimpleTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UserService _userService;

    public UserServiceSimpleTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockUnitOfWork.Setup(x => x.Usuarios).Returns(_mockUsuarioRepository.Object);
        _mockConfiguration.Setup(x => x["JWT:Secret"]).Returns("MinhaChaveSecretaSuperSeguraParaJWT2024!@#$%^&*()_+");

        _userService = new UserService(_mockUnitOfWork.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var usuarios = new List<Usuario>();
        
        var usuario1 = new Usuario
        {
            Id = "507f1f77bcf86cd799439011",
            Nome = "User 1",
            Role = UserRole.User,
            Avatar = "avatar1.jpg",
            Departamento = "IT",
            Ativo = true,
            DataCriacao = DateTime.UtcNow.AddDays(-30),
            DataAtualizacao = DateTime.UtcNow
        };
        usuario1.Email = new Email("user1@test.com");
        usuarios.Add(usuario1);
        
        var usuario2 = new Usuario
        {
            Id = "507f1f77bcf86cd799439012",
            Nome = "User 2",
            Role = UserRole.Admin,
            Avatar = "avatar2.jpg",
            Departamento = "HR",
            Ativo = true,
            DataCriacao = DateTime.UtcNow.AddDays(-20),
            DataAtualizacao = DateTime.UtcNow.AddDays(-1)
        };
        usuario2.Email = new Email("user2@test.com");
        usuarios.Add(usuario2);

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(usuarios);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var userList = result.ToList();
        userList[0].Name.Should().Be("User 1");
        userList[0].Email.Should().Be("user1@test.com");
        userList[0].Role.Should().Be("user");
        userList[1].Name.Should().Be("User 2");
        userList[1].Role.Should().Be("admin");
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";
        var usuario = new Usuario
        {
            Id = userId,
            Nome = "Test User",
            Role = UserRole.Manager,
            Avatar = "avatar.jpg",
            Departamento = "Sales",
            Ativo = true,
            DataCriacao = DateTime.UtcNow.AddDays(-15),
            DataAtualizacao = DateTime.UtcNow.AddHours(-2)
        };
        usuario.Email = new Email("test@test.com");

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(usuario);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@test.com");
        result.Role.Should().Be("manager");
        result.Avatar.Should().Be("avatar.jpg");
        result.Department.Should().Be("Sales");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(usuario.DataCriacao);
        result.UpdatedAt.Should().Be(usuario.DataAtualizacao);
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var createUserDto = new UserCreateDto
        {
            Name = "New User",
            Email = "existing@test.com",
            Password = "password123",
            Avatar = "avatar.jpg",
            Department = "Marketing",
            Role = "user"
        };

        var existingUsuario = new Usuario
        {
            Id = "507f1f77bcf86cd799439011",
            Nome = "Existing User",
            Role = UserRole.User,
            Avatar = "existing.jpg",
            Departamento = "IT",
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
        existingUsuario.Email = new Email("existing@test.com");

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Usuario> { existingUsuario });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _userService.CreateUserAsync(createUserDto));

        exception.Message.Should().Be("Email já está em uso");
    }

    [Fact]
    public async Task DeactivateUserAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _userService.DeactivateUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateUserAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = "507f1f77bcf86cd799439011";

        _mockUsuarioRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _userService.ActivateUserAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersByRoleAsync_InvalidRole_ShouldReturnEmptyList()
    {
        // Arrange
        var usuarios = new List<Usuario>();
        
        var usuario = new Usuario
        {
            Id = "507f1f77bcf86cd799439011",
            Nome = "Test User",
            Role = UserRole.User,
            Avatar = "avatar.jpg",
            Departamento = "IT",
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
        usuario.Email = new Email("test@test.com");
        usuarios.Add(usuario);

        _mockUsuarioRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(usuarios);

        // Act
        var result = await _userService.GetUsersByRoleAsync("invalidrole");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}