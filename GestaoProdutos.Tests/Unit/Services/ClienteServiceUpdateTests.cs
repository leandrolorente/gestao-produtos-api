using Xunit;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;
using GestaoProdutos.Domain.Interfaces;
using Moq;

namespace GestaoProdutos.Tests.Unit.Services;

public class ClienteServiceUpdateTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IEnderecoRepository> _mockEnderecoRepository;
    private readonly ClienteService _clienteService;

    public ClienteServiceUpdateTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockEnderecoRepository = new Mock<IEnderecoRepository>();
        
        _mockUnitOfWork.Setup(u => u.Clientes).Returns(_mockClienteRepository.Object);
        _mockUnitOfWork.Setup(u => u.Enderecos).Returns(_mockEnderecoRepository.Object);
        _clienteService = new ClienteService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task UpdateClienteAsync_WhenChangingFromCpfToCnpj_ShouldUpdateTipoCorrectly()
    {
        // Arrange
        var clienteId = "507f1f77bcf86cd799439011";
        var clienteExistente = new Cliente
        {
            Id = clienteId,
            Nome = "João Silva",
            Email = new Email("joao@teste.com"),
            Telefone = "(11) 99999-9999",
            CpfCnpj = new CpfCnpj("12345678901"), // CPF
            Tipo = TipoCliente.PessoaFisica,
            EnderecoId = "endereco1"
        };

        var updateDto = new UpdateClienteDto
        {
            Nome = "Empresa Silva LTDA",
            Email = "empresa@silva.com.br",
            Telefone = "(11) 3333-3333",
            CpfCnpj = "12345678000195", // CNPJ
            Endereco = new UpdateEnderecoDto
            {
                Logradouro = "Av. Principal",
                Numero = "100",
                Localidade = "São Paulo",
                Uf = "SP",
                Cep = "01000001"
            }
        };

        var enderecoExistente = new EnderecoEntity
        {
            Id = "endereco1",
            Cep = "01000000",
            Logradouro = "Rua Antiga",
            Numero = "100",
            Localidade = "São Paulo",
            Uf = "SP"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync(clienteExistente);
        _mockEnderecoRepository.Setup(r => r.GetByIdAsync("endereco1"))
            .ReturnsAsync(enderecoExistente);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var resultado = await _clienteService.UpdateClienteAsync(clienteId, updateDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Empresa Silva LTDA");
        resultado.Email.Should().Be("empresa@silva.com.br");
        resultado.CpfCnpj.Should().Be("12345678000195");
        resultado.Tipo.Should().Be("Pessoa Jurídica");

        // Verificar se o cliente foi atualizado corretamente
        clienteExistente.Tipo.Should().Be(TipoCliente.PessoaJuridica);
        clienteExistente.CpfCnpj!.EhCnpj.Should().BeTrue();
        
        _mockClienteRepository.Verify(r => r.UpdateAsync(clienteExistente), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateClienteAsync_WhenChangingFromCnpjToCpf_ShouldUpdateTipoCorrectly()
    {
        // Arrange
        var clienteId = "507f1f77bcf86cd799439011";
        var clienteExistente = new Cliente
        {
            Id = clienteId,
            Nome = "Empresa ABC LTDA",
            Email = new Email("contato@abc.com.br"),
            Telefone = "(11) 3333-3333",
            CpfCnpj = new CpfCnpj("12345678000195"), // CNPJ
            Tipo = TipoCliente.PessoaJuridica,
            EnderecoId = "endereco1"
        };

        var updateDto = new UpdateClienteDto
        {
            Nome = "Maria Silva",
            Email = "maria@teste.com",
            Telefone = "(11) 99999-9999",
            CpfCnpj = "12345678901", // CPF
            Endereco = new UpdateEnderecoDto
            {
                Logradouro = "Rua das Flores",
                Numero = "50",
                Localidade = "Rio de Janeiro",
                Uf = "RJ",
                Cep = "20000000"
            }
        };

        var enderecoExistente2 = new EnderecoEntity
        {
            Id = "endereco1",
            Cep = "01310100",
            Logradouro = "Av. Paulista",
            Numero = "1000",
            Localidade = "São Paulo",
            Uf = "SP"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync(clienteExistente);
        _mockEnderecoRepository.Setup(r => r.GetByIdAsync("endereco1"))
            .ReturnsAsync(enderecoExistente2);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var resultado = await _clienteService.UpdateClienteAsync(clienteId, updateDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be("Maria Silva");
        resultado.Email.Should().Be("maria@teste.com");
        resultado.CpfCnpj.Should().Be("12345678901");
        resultado.Tipo.Should().Be("Pessoa Física");

        // Verificar se o cliente foi atualizado corretamente
        clienteExistente.Tipo.Should().Be(TipoCliente.PessoaFisica);
        clienteExistente.CpfCnpj!.EhCpf.Should().BeTrue();
        
        _mockClienteRepository.Verify(r => r.UpdateAsync(clienteExistente), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateClienteAsync_WithEmptyCpfCnpj_ShouldNotChangeTipo()
    {
        // Arrange
        var clienteId = "507f1f77bcf86cd799439011";
        var tipoOriginal = TipoCliente.PessoaFisica;
        var clienteExistente = new Cliente
        {
            Id = clienteId,
            Nome = "João Silva",
            Email = new Email("joao@teste.com"),
            CpfCnpj = new CpfCnpj("12345678901"),
            Tipo = tipoOriginal,
            EnderecoId = "endereco1"
        };

        var updateDto = new UpdateClienteDto
        {
            Nome = "João Silva Atualizado",
            Email = "joao.novo@teste.com",
            CpfCnpj = "", // CPF/CNPJ vazio
            Endereco = new UpdateEnderecoDto
            {
                Logradouro = "Nova Rua",
                Numero = "100",
                Localidade = "São Paulo",
                Uf = "SP",
                Cep = "01000000"
            }
        };

        var enderecoExistente3 = new EnderecoEntity
        {
            Id = "endereco1",
            Cep = "01000000",
            Logradouro = "Rua Antiga",
            Numero = "100",
            Localidade = "São Paulo",
            Uf = "SP"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync(clienteExistente);
        _mockEnderecoRepository.Setup(r => r.GetByIdAsync("endereco1"))
            .ReturnsAsync(enderecoExistente3);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var resultado = await _clienteService.UpdateClienteAsync(clienteId, updateDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.CpfCnpj.Should().Be(string.Empty);
        resultado.Tipo.Should().Be("Pessoa Física"); // Deve manter o tipo original

        clienteExistente.Tipo.Should().Be(tipoOriginal);
        clienteExistente.CpfCnpj.Should().BeNull();
    }

    [Fact]
    public async Task UpdateClienteAsync_WhenClienteNotFound_ShouldThrowArgumentException()
    {
        // Arrange
        var clienteId = "507f1f77bcf86cd799439011";
        var updateDto = new UpdateClienteDto { Nome = "Teste" };

        _mockClienteRepository.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _clienteService.UpdateClienteAsync(clienteId, updateDto));
        
        exception.Message.Should().Be("Cliente não encontrado");
    }

    [Theory]
    [InlineData("12345678901", "Pessoa Física")]  // CPF
    [InlineData("12345678000195", "Pessoa Jurídica")]  // CNPJ
    public async Task UpdateClienteAsync_ShouldReturnCorrectTipoInDto(string cpfCnpjValue, string expectedTipoDescription)
    {
        // Arrange
        var clienteId = "507f1f77bcf86cd799439011";
        var clienteExistente = new Cliente
        {
            Id = clienteId,
            Nome = "Cliente Teste",
            Email = new Email("teste@teste.com"),
            CpfCnpj = new CpfCnpj("11111111111"),
            Tipo = TipoCliente.PessoaFisica,
            EnderecoId = "endereco1"
        };

        var updateDto = new UpdateClienteDto
        {
            Nome = "Cliente Atualizado",
            Email = "atualizado@teste.com",
            CpfCnpj = cpfCnpjValue,
            Endereco = new UpdateEnderecoDto
            {
                Logradouro = "Rua Teste",
                Numero = "100",
                Localidade = "Cidade Teste",
                Uf = "SP",
                Cep = "00000000"
            }
        };

        var enderecoExistente4 = new EnderecoEntity
        {
            Id = "endereco1",
            Cep = "00000000",
            Logradouro = "Rua Antiga",
            Numero = "100",
            Localidade = "Cidade Teste",
            Uf = "SP"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync(clienteId))
            .ReturnsAsync(clienteExistente);
        _mockEnderecoRepository.Setup(r => r.GetByIdAsync("endereco1"))
            .ReturnsAsync(enderecoExistente4);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var resultado = await _clienteService.UpdateClienteAsync(clienteId, updateDto);

        // Assert
        resultado.Tipo.Should().Be(expectedTipoDescription);
        resultado.CpfCnpj.Should().Be(cpfCnpjValue);
    }
}