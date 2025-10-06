using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace GestaoProdutos.Tests.Unit.Services;

public class FornecedorServiceTests
{
    private readonly Mock<IFornecedorRepository> _fornecedorRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<FornecedorService>> _loggerMock;
    private readonly FornecedorService _fornecedorService;

    public FornecedorServiceTests()
    {
        _fornecedorRepositoryMock = new Mock<IFornecedorRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<FornecedorService>>();
        
        _fornecedorService = new FornecedorService(
            _fornecedorRepositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    private static Fornecedor CriarFornecedorTeste()
    {
        return new Fornecedor
        {
            Id = "123456789012345678901234",
            RazaoSocial = "Fornecedor Teste LTDA",
            NomeFantasia = "Fornecedor Teste",
            CnpjCpf = new CpfCnpj("12.345.678/0001-90"),
            Email = new Email("contato@fornecedorteste.com"),
            Telefone = "(11) 99999-9999",
            Tipo = TipoFornecedor.Nacional,
            Status = StatusFornecedor.Ativo,
            PrazoPagamentoPadrao = 30,
            LimiteCredito = 50000m,
            TotalComprado = 25000m,
            QuantidadeCompras = 5,
            UltimaCompra = DateTime.UtcNow.AddDays(-15),
            DataCriacao = DateTime.UtcNow.AddDays(-30),
            DataAtualizacao = DateTime.UtcNow.AddDays(-1),
            Ativo = true
        };
    }

    [Fact]
    public async Task GetAllFornecedoresAsync_ComCacheDisponivel_DeveRetornarDoCache()
    {
        // Arrange
        var fornecedoresDto = new List<FornecedorDto>
        {
            new FornecedorDto { Id = "1", RazaoSocial = "Fornecedor 1" },
            new FornecedorDto { Id = "2", RazaoSocial = "Fornecedor 2" }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<FornecedorDto>>("gp:fornecedores:all"))
            .ReturnsAsync(fornecedoresDto);

        // Act
        var resultado = await _fornecedorService.GetAllFornecedoresAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().BeEquivalentTo(fornecedoresDto);
        _fornecedorRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllFornecedoresAsync_SemCache_DeveBuscarNoBancoESalvarNoCache()
    {
        // Arrange
        var fornecedores = new List<Fornecedor> { CriarFornecedorTeste() };

        _cacheServiceMock.Setup(x => x.GetAsync<List<FornecedorDto>>("gp:fornecedores:all"))
            .ReturnsAsync((List<FornecedorDto>?)null);

        _fornecedorRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _fornecedorService.GetAllFornecedoresAsync();

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().RazaoSocial.Should().Be("Fornecedor Teste LTDA");
        
        _fornecedorRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync("gp:fornecedores:all", It.IsAny<List<FornecedorDto>>(), TimeSpan.FromMinutes(5)), Times.Once);
    }

    [Fact]
    public async Task GetFornecedorByIdAsync_ComFornecedorExistente_DeveRetornarDto()
    {
        // Arrange
        var fornecedor = CriarFornecedorTeste();
        var id = "123456789012345678901234";

        _cacheServiceMock.Setup(x => x.GetAsync<FornecedorDto>($"gp:fornecedor:{id}"))
            .ReturnsAsync((FornecedorDto?)null);

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.GetFornecedorByIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.RazaoSocial.Should().Be("Fornecedor Teste LTDA");
        resultado.CnpjCpf.Should().Be("12.345.678/0001-90");
        resultado.Email.Should().Be("contato@fornecedorteste.com");
    }

    [Fact]
    public async Task GetFornecedorByIdAsync_ComFornecedorInexistente_DeveRetornarNull()
    {
        // Arrange
        var id = "123456789012345678901234";

        _cacheServiceMock.Setup(x => x.GetAsync<FornecedorDto>($"gp:fornecedor:{id}"))
            .ReturnsAsync((FornecedorDto?)null);

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Fornecedor?)null);

        // Act
        var resultado = await _fornecedorService.GetFornecedorByIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CreateFornecedorAsync_ComDadosValidos_DeveCriarFornecedor()
    {
        // Arrange
        var dto = new CreateFornecedorDto
        {
            RazaoSocial = "Novo Fornecedor LTDA",
            NomeFantasia = "Novo Fornecedor",
            CnpjCpf = "98.765.432/0001-10",
            Email = "novo@fornecedor.com",
            Telefone = "(11) 88888-8888",
            Tipo = TipoFornecedor.Nacional,
            PrazoPagamentoPadrao = 30,
            LimiteCredito = 100000m
        };

        var fornecedorCriado = CriarFornecedorTeste();

        _fornecedorRepositoryMock.Setup(x => x.CnpjCpfJaExisteAsync(dto.CnpjCpf, null))
            .ReturnsAsync(false);

        _fornecedorRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedorCriado);

        // Act
        var resultado = await _fornecedorService.CreateFornecedorAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.RazaoSocial.Should().Be("Fornecedor Teste LTDA");
        
        _fornecedorRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Fornecedor>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("gp:fornecedores:all"), Times.Once);
    }

    [Fact]
    public async Task CreateFornecedorAsync_ComCnpjExistente_DeveLancarExcecao()
    {
        // Arrange
        var dto = new CreateFornecedorDto
        {
            RazaoSocial = "Novo Fornecedor LTDA",
            CnpjCpf = "12.345.678/0001-90",
            Email = "novo@fornecedor.com",
            Telefone = "(11) 99999-9999",
            Tipo = TipoFornecedor.Nacional,
            PrazoPagamentoPadrao = 30,
            LimiteCredito = 0
        };

        _fornecedorRepositoryMock.Setup(x => x.CnpjCpfJaExisteAsync(dto.CnpjCpf, null))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _fornecedorService.CreateFornecedorAsync(dto));
        
        exception.Message.Should().Be("Já existe um fornecedor com o CNPJ/CPF: 12.345.678/0001-90");
    }

    [Fact]
    public async Task UpdateFornecedorAsync_ComFornecedorExistente_DeveAtualizarFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var fornecedor = CriarFornecedorTeste();
        
        var dto = new UpdateFornecedorDto
        {
            RazaoSocial = "Fornecedor Atualizado LTDA",
            NomeFantasia = "Fornecedor Atualizado",
            Email = "atualizado@fornecedor.com",
            Telefone = "(11) 77777-7777",
            Tipo = TipoFornecedor.Internacional,
            Status = StatusFornecedor.Ativo,
            PrazoPagamentoPadrao = 45,
            LimiteCredito = 75000m
        };

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.UpdateFornecedorAsync(id, dto);

        // Assert
        resultado.Should().NotBeNull();
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("gp:fornecedores:all"), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"gp:fornecedor:{id}"), Times.Once);
    }

    [Fact]
    public async Task UpdateFornecedorAsync_ComFornecedorInexistente_DeveLancarExcecao()
    {
        // Arrange
        var id = "123456789012345678901234";
        var dto = new UpdateFornecedorDto { RazaoSocial = "Teste" };

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((Fornecedor?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _fornecedorService.UpdateFornecedorAsync(id, dto));
        
        exception.Message.Should().Be("Fornecedor não encontrado");
    }

    [Fact]
    public async Task DeleteFornecedorAsync_ComFornecedorExistente_DeveExcluirFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";

        _fornecedorRepositoryMock.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _fornecedorService.DeleteFornecedorAsync(id);

        // Assert
        resultado.Should().BeTrue();
        _fornecedorRepositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("gp:fornecedores:all"), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync($"gp:fornecedor:{id}"), Times.Once);
    }

    [Fact]
    public async Task BloquearFornecedorAsync_ComFornecedorExistente_DeveBloquearFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var motivo = "Inadimplência";
        var fornecedor = CriarFornecedorTeste();

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.BloquearFornecedorAsync(id, motivo);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.Status.Should().Be(StatusFornecedor.Bloqueado);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task DesbloquearFornecedorAsync_ComFornecedorBloqueado_DeveDesbloquearFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var fornecedor = CriarFornecedorTeste();
        fornecedor.Bloquear("Teste");

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.DesbloquearFornecedorAsync(id);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.Status.Should().Be(StatusFornecedor.Ativo);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task InativarFornecedorAsync_ComFornecedorExistente_DeveInativarFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var fornecedor = CriarFornecedorTeste();

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.InativarFornecedorAsync(id);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.Status.Should().Be(StatusFornecedor.Inativo);
        fornecedor.Ativo.Should().BeFalse();
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task AtivarFornecedorAsync_ComFornecedorInativo_DeveAtivarFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var fornecedor = CriarFornecedorTeste();
        fornecedor.Inativar();

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.AtivarFornecedorAsync(id);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.Status.Should().Be(StatusFornecedor.Ativo);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarProdutoAsync_ComFornecedorExistente_DeveAdicionarProduto()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var produtoId = "produto123";
        var fornecedor = CriarFornecedorTeste();

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(fornecedorId))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.AdicionarProdutoAsync(fornecedorId, produtoId);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.ProdutoIds.Should().Contain(produtoId);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task RemoverProdutoAsync_ComProdutoExistente_DeveRemoverProduto()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var produtoId = "produto123";
        var fornecedor = CriarFornecedorTeste();
        fornecedor.AdicionarProduto(produtoId);

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(fornecedorId))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.RemoverProdutoAsync(fornecedorId, produtoId);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.ProdutoIds.Should().NotContain(produtoId);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarCompraAsync_ComFornecedorExistente_DeveRegistrarCompra()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var valor = 5000m;
        var fornecedor = CriarFornecedorTeste();
        var totalAnterior = fornecedor.TotalComprado;

        _fornecedorRepositoryMock.Setup(x => x.GetByIdAsync(fornecedorId))
            .ReturnsAsync(fornecedor);

        _fornecedorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Fornecedor>()))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _fornecedorService.RegistrarCompraAsync(fornecedorId, valor);

        // Assert
        resultado.Should().BeTrue();
        fornecedor.TotalComprado.Should().Be(totalAnterior + valor);
        _fornecedorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Fornecedor>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync("gp:fornecedores:all"), Times.Once);
    }

    [Fact]
    public async Task GetFornecedoresPorTipoAsync_DeveRetornarFornecedoresDoTipo()
    {
        // Arrange
        var tipo = TipoFornecedor.Nacional;
        var fornecedores = new List<Fornecedor> { CriarFornecedorTeste() };

        _fornecedorRepositoryMock.Setup(x => x.GetFornecedoresAtivosPorTipoAsync(tipo))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _fornecedorService.GetFornecedoresAtivosPorTipoAsync(tipo);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Tipo.Should().Be(tipo.ToString());
    }

    [Fact]
    public async Task GetFornecedoresPorStatusAsync_DeveRetornarFornecedoresDoStatus()
    {
        // Arrange
        var status = StatusFornecedor.Ativo;
        var fornecedores = new List<Fornecedor> { CriarFornecedorTeste() };

        _fornecedorRepositoryMock.Setup(x => x.GetFornecedoresPorStatusAsync(status))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _fornecedorService.GetFornecedoresPorStatusAsync(status);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Status.Should().Be(status.ToString());
    }

    [Fact]
    public async Task BuscarFornecedoresAsync_DeveRetornarFornecedoresFiltrados()
    {
        // Arrange
        var termo = "Teste";
        var fornecedores = new List<Fornecedor> { CriarFornecedorTeste() };

        _fornecedorRepositoryMock.Setup(x => x.BuscarFornecedoresAsync(termo))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _fornecedorService.BuscarFornecedoresAsync(termo);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().RazaoSocial.Should().Contain("Teste");
    }
}