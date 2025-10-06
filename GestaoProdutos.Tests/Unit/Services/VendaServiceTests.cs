using Xunit;
using Moq;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;

namespace GestaoProdutos.Tests.Unit.Services;

public class VendaServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IVendaRepository> _mockVendaRepository;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IProdutoRepository> _mockProdutoRepository;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly VendaService _vendaService;

    public VendaServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockVendaRepository = new Mock<IVendaRepository>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockProdutoRepository = new Mock<IProdutoRepository>();
        _mockUsuarioRepository = new Mock<IUsuarioRepository>();

        _mockUnitOfWork.Setup(u => u.Vendas).Returns(_mockVendaRepository.Object);
        _mockUnitOfWork.Setup(u => u.Clientes).Returns(_mockClienteRepository.Object);
        _mockUnitOfWork.Setup(u => u.Produtos).Returns(_mockProdutoRepository.Object);
        _mockUnitOfWork.Setup(u => u.Usuarios).Returns(_mockUsuarioRepository.Object);

        _vendaService = new VendaService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllVendasAsync_DeveRetornarTodasVendas()
    {
        // Arrange
        var vendas = new List<Venda>
        {
            CriarVendaMock("1", "VND-001"),
            CriarVendaMock("2", "VND-002")
        };

        _mockVendaRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(vendas);

        // Act
        var resultado = await _vendaService.GetAllVendasAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().Contain(v => v.Numero == "VND-001");
        resultado.Should().Contain(v => v.Numero == "VND-002");
    }

    [Fact]
    public async Task GetVendaByIdAsync_ComIdValido_DeveRetornarVenda()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001");
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(venda);

        // Act
        var resultado = await _vendaService.GetVendaByIdAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be("1");
        resultado.Numero.Should().Be("VND-001");
    }

    [Fact]
    public async Task GetVendaByIdAsync_ComIdInvalido_DeveRetornarNull()
    {
        // Arrange
        _mockVendaRepository.Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((Venda?)null);

        // Act
        var resultado = await _vendaService.GetVendaByIdAsync("999");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CreateVendaAsync_ComDadosValidos_DeveCriarVenda()
    {
        // Arrange
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 10, 100.00m);
        var vendedor = CriarUsuarioMock("1", "Maria Vendedora");

        var createDto = new CreateVendaDto
        {
            ClienteId = "1",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto
                {
                    ProdutoId = "1",
                    ProdutoNome = "Produto A",
                    ProdutoSku = "PRD-001",
                    Quantidade = 2,
                    PrecoUnitario = 100.00m,
                    Subtotal = 200.00m
                }
            },
            Desconto = 10.00m,
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(produto);
        _mockUsuarioRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(vendedor);
        _mockVendaRepository.Setup(r => r.GetProximoNumeroVendaAsync())
            .ReturnsAsync("VND-001");
        _mockVendaRepository.Setup(r => r.CreateAsync(It.IsAny<Venda>()))
            .ReturnsAsync((Venda v) => v);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var resultado = await _vendaService.CreateVendaAsync(createDto, "1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.ClienteId.Should().Be("1");
        resultado.ClienteNome.Should().Be("João Silva");
        resultado.Items.Should().HaveCount(1);
        resultado.Total.Should().Be(190.00m); // 200 - 10 de desconto
        resultado.FormaPagamento.Should().Be("PIX");
        resultado.Status.Should().Be("Pendente");

        // Verify
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Quantidade == 8)), Times.Once);
    }

    [Fact]
    public async Task CreateVendaAsync_ComClienteInexistente_DeveLancarExcecao()
    {
        // Arrange
        var createDto = new CreateVendaDto
        {
            ClienteId = "999",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto
                {
                    ProdutoId = "1",
                    Quantidade = 1,
                    PrecoUnitario = 100.00m
                }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await _vendaService.Invoking(s => s.CreateVendaAsync(createDto, "1"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cliente não encontrado");
    }

    [Fact]
    public async Task CreateVendaAsync_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 1, 100.00m); // Apenas 1 em estoque

        var createDto = new CreateVendaDto
        {
            ClienteId = "1",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto
                {
                    ProdutoId = "1",
                    ProdutoNome = "Produto A",
                    ProdutoSku = "PRD-001",
                    Quantidade = 5, // Solicitando 5, mas só tem 1
                    PrecoUnitario = 100.00m,
                    Subtotal = 500.00m
                }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(produto);

        // Act & Assert
        await _vendaService.Invoking(s => s.CreateVendaAsync(createDto, "1"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Estoque insuficiente para o produto Produto A. Disponível: 1");
    }

    [Fact]
    public async Task ConfirmarVendaAsync_ComVendaPendente_DeveConfirmarVenda()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001");
        venda.Status = StatusVenda.Pendente;

        _mockVendaRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(venda);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>()))
            .ReturnsAsync((Venda v) => v);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var resultado = await _vendaService.ConfirmarVendaAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("Confirmada");
        _mockVendaRepository.Verify(r => r.UpdateAsync(It.Is<Venda>(v => v.Status == StatusVenda.Confirmada)), Times.Once);
    }

    [Fact]
    public async Task CancelarVendaAsync_ComVendaPendente_DeveCancelarVenda()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001");
        venda.Status = StatusVenda.Pendente;
        venda.Items = new List<VendaItem>
        {
            new VendaItem("1", "Produto A", "PRD-001", 2, 100.00m)
        };

        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 8, 100.00m);

        _mockVendaRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(venda);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(produto);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>()))
            .ReturnsAsync((Venda v) => v);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var resultado = await _vendaService.CancelarVendaAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("Cancelada");
        
        // Verify que o estoque foi restaurado
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Quantidade == 10)), Times.Once);
    }

    [Fact]
    public async Task GetProximoNumeroVendaAsync_DeveRetornarProximoNumero()
    {
        // Arrange
        _mockVendaRepository.Setup(r => r.GetProximoNumeroVendaAsync())
            .ReturnsAsync("VND-005");

        // Act
        var resultado = await _vendaService.GetProximoNumeroVendaAsync();

        // Assert
        resultado.Should().Be("VND-005");
    }

    // Métodos auxiliares para criar mocks
    private static Venda CriarVendaMock(string id, string numero)
    {
        return new Venda
        {
            Id = id,
            Numero = numero,
            ClienteId = "1",
            ClienteNome = "Cliente Teste",
            ClienteEmail = "cliente@teste.com",
            Items = new List<VendaItem>(),
            Subtotal = 100.00m,
            Desconto = 0,
            Total = 100.00m,
            FormaPagamento = FormaPagamento.PIX,
            Status = StatusVenda.Pendente,
            DataVenda = DateTime.UtcNow,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            Ativo = true
        };
    }

    private static Cliente CriarClienteMock(string id, string nome, string email)
    {
        return new Cliente
        {
            Id = id,
            Nome = nome,
            Email = new GestaoProdutos.Domain.ValueObjects.Email(email),
            CpfCnpj = new GestaoProdutos.Domain.ValueObjects.CpfCnpj("12345678901"),
            Telefone = "(11) 99999-9999",
            EnderecoId = "endereco_teste",
            Tipo = TipoCliente.PessoaFisica,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
    }

    private static Produto CriarProdutoMock(string id, string nome, string sku, int quantidade, decimal preco)
    {
        return new Produto
        {
            Id = id,
            Nome = nome,
            Sku = sku,
            Quantidade = quantidade,
            Preco = preco,
            Categoria = "Categoria Teste",
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
    }

    private static Usuario CriarUsuarioMock(string id, string nome)
    {
        return new Usuario
        {
            Id = id,
            Nome = nome,
            Email = new GestaoProdutos.Domain.ValueObjects.Email("usuario@teste.com"),
            Avatar = "avatar.jpg",
            Departamento = "Vendas",
            Role = UserRole.User,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };
    }
}