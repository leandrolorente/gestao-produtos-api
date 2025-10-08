using Xunit;
using Moq;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.ValueObjects;

namespace GestaoProdutos.Tests.Unit.Services;

public class VendaServiceAdvancedTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IVendaRepository> _mockVendaRepository;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IProdutoRepository> _mockProdutoRepository;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepository;
    private readonly VendaService _vendaService;

    public VendaServiceAdvancedTests()
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
    public async Task CreateVendaAsync_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 5, 100.00m); // Estoque 5 e preço válido
        
        var createDto = new CreateVendaDto
        {
            ClienteId = "1",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto 
                { 
                    ProdutoId = "1", 
                    Quantidade = 10, 
                    PrecoUnitario = 100.00m // Adicionar preço
                }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(produto);
        _mockVendaRepository.Setup(r => r.GetProximoNumeroVendaAsync()).ReturnsAsync("VND-001");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _vendaService.CreateVendaAsync(createDto, "vendedor1"));
        
        exception.Message.Should().Contain("Estoque insuficiente");
    }

    [Fact]
    public async Task CreateVendaAsync_ComClienteInexistente_DeveLancarExcecao()
    {
        // Arrange
        var createDto = new CreateVendaDto
        {
            ClienteId = "999", // Cliente inexistente
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto { ProdutoId = "1", Quantidade = 1 }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("999")).ReturnsAsync((Cliente?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _vendaService.CreateVendaAsync(createDto, "vendedor1"));
        
        exception.Message.Should().Contain("Cliente não encontrado");
    }

    [Fact]
    public async Task CreateVendaAsync_ComProdutoInexistente_DeveLancarExcecao()
    {
        // Arrange
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        
        var createDto = new CreateVendaDto
        {
            ClienteId = "1",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto 
                { 
                    ProdutoId = "999", 
                    Quantidade = 1,
                    PrecoUnitario = 100.00m // Adicionar preço
                }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("999")).ReturnsAsync((Produto?)null);
        _mockVendaRepository.Setup(r => r.GetProximoNumeroVendaAsync()).ReturnsAsync("VND-001");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _vendaService.CreateVendaAsync(createDto, "vendedor1"));
        
        exception.Message.Should().Contain("Produto não encontrado");
    }

    [Fact]
    public async Task CreateVendaAsync_ComSucesso_DeveAtualizarEstoque()
    {
        // Arrange
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 10, 100.00m);
        
        var createDto = new CreateVendaDto
        {
            ClienteId = "1",
            Items = new List<CreateVendaItemDto>
            {
                new CreateVendaItemDto 
                { 
                    ProdutoId = "1", 
                    Quantidade = 3,
                    PrecoUnitario = 100.00m // Adicionar preço
                }
            },
            FormaPagamento = "PIX"
        };

        _mockClienteRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(produto);
        _mockVendaRepository.Setup(r => r.GetProximoNumeroVendaAsync()).ReturnsAsync("VND-001");
        _mockVendaRepository.Setup(r => r.CreateAsync(It.IsAny<Venda>())).ReturnsAsync((Venda v) => v);

        // Act
        var resultado = await _vendaService.CreateVendaAsync(createDto, "vendedor1");

        // Assert
        resultado.Should().NotBeNull();
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Quantidade == 7)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmarVendaAsync_ComVendaPendente_DeveConfirmar()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001", StatusVenda.Pendente);
        
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(venda);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>())).ReturnsAsync((Venda v) => v);

        // Act
        var resultado = await _vendaService.ConfirmarVendaAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("Confirmada");
        _mockVendaRepository.Verify(r => r.UpdateAsync(It.Is<Venda>(v => v.Status == StatusVenda.Confirmada)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmarVendaAsync_ComVendaJaConfirmada_DeveLancarExcecao()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001", StatusVenda.Confirmada);
        
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(venda);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _vendaService.ConfirmarVendaAsync("1"));
        
        exception.Message.Should().Contain("Apenas vendas pendentes podem ser confirmadas");
    }

    [Fact]
    public async Task FinalizarVendaAsync_ComVendaConfirmada_DeveFinalizarEGerarRecibo()
    {
        // Arrange
        var venda = CriarVendaMock("1", "VND-001", StatusVenda.Confirmada);
        
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(venda);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>())).ReturnsAsync((Venda v) => v);

        // Act
        var resultado = await _vendaService.FinalizarVendaAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("Finalizada");
        _mockVendaRepository.Verify(r => r.UpdateAsync(It.Is<Venda>(v => v.Status == StatusVenda.Finalizada)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelarVendaAsync_ComVendaPendente_DeveCancelarERestaurarEstoque()
    {
        // Arrange
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 7, 100.00m); // Estoque atual após venda
        var venda = CriarVendaMockComItens("1", "VND-001", StatusVenda.Pendente, produto.Id, 3);
        
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(venda);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>())).ReturnsAsync((Venda v) => v);

        // Act
        var resultado = await _vendaService.CancelarVendaAsync("1");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("Cancelada");
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Quantidade == 10)), Times.Once); // Restaurou estoque
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetVendasPorStatusAsync_ComStatusValido_DeveRetornarVendasFiltradas()
    {
        // Arrange
        var vendas = new List<Venda>
        {
            CriarVendaMock("1", "VND-001", StatusVenda.Pendente),
            CriarVendaMock("2", "VND-002", StatusVenda.Confirmada),
            CriarVendaMock("3", "VND-003", StatusVenda.Pendente)
        };

        _mockVendaRepository.Setup(r => r.GetVendasPorStatusAsync(StatusVenda.Pendente))
            .ReturnsAsync(vendas.Where(v => v.Status == StatusVenda.Pendente));

        // Act
        var resultado = await _vendaService.GetVendasPorStatusAsync("Pendente");

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(v => v.Status == "Pendente");
    }

    [Fact]
    public async Task GetVendasPorStatusAsync_ComStatusInvalido_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _vendaService.GetVendasPorStatusAsync("StatusInvalido"));
        
        exception.Message.Should().Contain("Status inválido");
    }

    [Fact]
    public async Task GetVendasPorPeriodoAsync_ComPeriodoValido_DeveRetornarVendasDoPeriodo()
    {
        // Arrange
        var dataInicio = DateTime.Today.AddDays(-7);
        var dataFim = DateTime.Today;
        
        var vendas = new List<Venda>
        {
            CriarVendaMock("1", "VND-001", StatusVenda.Finalizada, dataInicio.AddDays(1)),
            CriarVendaMock("2", "VND-002", StatusVenda.Confirmada, dataInicio.AddDays(-2)), // Fora do período
            CriarVendaMock("3", "VND-003", StatusVenda.Pendente, dataFim)
        };

        _mockVendaRepository.Setup(r => r.GetVendasPorPeriodoAsync(dataInicio, dataFim))
            .ReturnsAsync(vendas.Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim));

        // Act
        var resultado = await _vendaService.GetVendasPorPeriodoAsync(dataInicio, dataFim);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(v => DateTime.Parse(v.DataVenda) >= dataInicio && DateTime.Parse(v.DataVenda) <= dataFim);
    }

    [Fact]
    public async Task UpdateVendaAsync_ComVendaPendente_DeveAtualizarCorretamente()
    {
        // Arrange
        var produtoOriginal = CriarProdutoMock("1", "Produto A", "PRD-001", 7, 100.00m);
        var produtoNovo = CriarProdutoMock("2", "Produto B", "PRD-002", 15, 150.00m);
        var cliente = CriarClienteMock("1", "João Silva", "joao@email.com");
        
        var vendaOriginal = CriarVendaMockComItens("1", "VND-001", StatusVenda.Pendente, produtoOriginal.Id, 3);
        
        var updateDto = new UpdateVendaDto
        {
            ClienteId = "1",
            Items = new List<VendaItemDto>
            {
                new VendaItemDto 
                { 
                    ProdutoId = "2", 
                    Quantidade = 2,
                    PrecoUnitario = 150.00m // Produto diferente
                }
            },
            Desconto = 10m,
            FormaPagamento = "CartaoCredito",
            Status = "Pendente" // Adicionar status válido
        };

        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(vendaOriginal);
        _mockClienteRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(cliente);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(produtoOriginal);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync("2")).ReturnsAsync(produtoNovo);
        _mockVendaRepository.Setup(r => r.UpdateAsync(It.IsAny<Venda>())).ReturnsAsync((Venda v) => v);

        // Act
        var resultado = await _vendaService.UpdateVendaAsync("1", updateDto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Desconto.Should().Be(10m);
        resultado.FormaPagamento.Should().Be("Cartão de Crédito");
        
        // Verificar restauração do estoque original e redução do novo
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Id == "1" && p.Quantidade == 10)), Times.Once); // Restaurado
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Id == "2" && p.Quantidade == 13)), Times.Once); // Reduzido
    }

    [Fact]
    public async Task DeleteVendaAsync_ComVendaPendente_DeveExcluirERestaurarEstoque()
    {
        // Arrange
        var produto = CriarProdutoMock("1", "Produto A", "PRD-001", 7, 100.00m);
        var venda = CriarVendaMockComItens("1", "VND-001", StatusVenda.Pendente, produto.Id, 3);
        
        _mockVendaRepository.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(venda);
        _mockProdutoRepository.Setup(r => r.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        _mockVendaRepository.Setup(r => r.DeleteAsync("1")).ReturnsAsync(true);

        // Act
        var resultado = await _vendaService.DeleteVendaAsync("1");

        // Assert
        resultado.Should().BeTrue();
        _mockProdutoRepository.Verify(r => r.UpdateAsync(It.Is<Produto>(p => p.Quantidade == 10)), Times.Once); // Restaurou estoque
        _mockVendaRepository.Verify(r => r.DeleteAsync("1"), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetVendasStatsAsync_DeveRetornarEstatisticasCorretas()
    {
        // Arrange
        // Reset mocks for this test
        _mockVendaRepository.Reset();
        
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var vendas = new List<Venda>
        {
            CriarVendaMock("1", "VND-001", StatusVenda.Finalizada, hoje, 1000m),
            CriarVendaMock("2", "VND-002", StatusVenda.Confirmada, hoje.AddDays(-1), 500m),
            CriarVendaMock("3", "VND-003", StatusVenda.Pendente, hoje.AddDays(-2), 300m)
        };

        var vendasHoje = vendas.Where(v => v.DataVenda.Date == hoje).ToList();
        var vendasPendentes = vendas.Where(v => v.Status == StatusVenda.Pendente).ToList();
        var topClientes = new List<TopClienteResult>
        {
            new() { ClienteNome = "Cliente 1", TotalCompras = 5, ValorTotal = 1000 }
        };
        var vendasPorMes = new List<VendasPorMesResult>
        {
            new() { Mes = "2024-01", Vendas = 10, Faturamento = 5000 }
        };

        _mockVendaRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(vendas);
        _mockVendaRepository.Setup(r => r.GetVendasHojeAsync()).ReturnsAsync(vendasHoje);
        _mockVendaRepository.Setup(r => r.GetFaturamentoPorPeriodoAsync(inicioMes, fimMes)).ReturnsAsync(1500m);
        _mockVendaRepository.Setup(r => r.GetVendasPorStatusAsync(StatusVenda.Pendente)).ReturnsAsync(vendasPendentes);
        _mockVendaRepository.Setup(r => r.GetTopClientesAsync(5)).ReturnsAsync(topClientes);
        _mockVendaRepository.Setup(r => r.GetVendasPorMesAsync(6)).ReturnsAsync(vendasPorMes);

        // Act
        var resultado = await _vendaService.GetVendasStatsAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalVendas.Should().Be(3);
        resultado.VendasHoje.Should().Be(1);
        resultado.FaturamentoMes.Should().Be(1500m);
        resultado.VendasPendentes.Should().Be(1);
        resultado.TicketMedio.Should().Be(1000m); // Apenas vendas finalizadas
        resultado.TopClientes.Should().HaveCount(1);
        resultado.VendasPorMes.Should().HaveCount(1);
    }

    // Métodos auxiliares
    private Cliente CriarClienteMock(string id, string nome, string email)
    {
        var cliente = new Cliente
        {
            Nome = nome,
            Email = new Email(email),
            CpfCnpj = new CpfCnpj("123.456.789-01"),
            Telefone = "11999999999",
            Tipo = TipoCliente.PessoaFisica
        };
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, id);
        return cliente;
    }

    private Produto CriarProdutoMock(string id, string nome, string sku, int quantidade, decimal preco)
    {
        var produto = new Produto
        {
            Nome = nome,
            Sku = sku,
            Quantidade = quantidade,
            Preco = preco,
            Categoria = "Eletronicos",
            EstoqueMinimo = 10
        };
        produto.GetType().GetProperty("Id")?.SetValue(produto, id);
        return produto;
    }

    private Venda CriarVendaMock(string id, string numero, StatusVenda status, DateTime? dataVenda = null, decimal total = 500m)
    {
        var venda = new Venda
        {
            Numero = numero,
            ClienteId = "cliente1",
            ClienteNome = "Cliente Teste",
            ClienteEmail = "cliente@teste.com",
            Items = new List<VendaItem>(),
            Subtotal = total,
            Desconto = 0,
            Total = total,
            FormaPagamento = FormaPagamento.PIX,
            Status = status,
            DataVenda = dataVenda ?? DateTime.UtcNow,
            VendedorId = "vendedor1",
            VendedorNome = "Vendedor Teste"
        };

        venda.GetType().GetProperty("Id")?.SetValue(venda, id);
        return venda;
    }

    private Venda CriarVendaMockComItens(string id, string numero, StatusVenda status, string produtoId, int quantidade)
    {
        var venda = CriarVendaMock(id, numero, status);
        venda.Items = new List<VendaItem>
        {
            new VendaItem
            {
                Id = "item1",
                ProdutoId = produtoId,
                ProdutoNome = "Produto Teste",
                ProdutoSku = "SKU-001",
                Quantidade = quantidade,
                PrecoUnitario = 100m,
                Subtotal = quantidade * 100m
            }
        };
        venda.Subtotal = venda.Items.Sum(i => i.Subtotal);
        venda.Total = venda.Subtotal;
        return venda;
    }
}
