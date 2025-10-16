using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace GestaoProdutos.Tests.Unit.Services;

/// <summary>
/// Testes unitários para o ContaPagarService
/// </summary>
public class ContaPagarServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ContaPagarService>> _loggerMock;
    private readonly Mock<IContaPagarRepository> _repositoryMock;
    private readonly Mock<IFornecedorRepository> _fornecedorRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly ContaPagarService _service;

    public ContaPagarServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ContaPagarService>>();
        _repositoryMock = new Mock<IContaPagarRepository>();
        _fornecedorRepositoryMock = new Mock<IFornecedorRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();

        _unitOfWorkMock.Setup(u => u.ContasPagar).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Fornecedores).Returns(_fornecedorRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Usuarios).Returns(_usuarioRepositoryMock.Object);

        _service = new ContaPagarService(_unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllContasPagarAsync_ComCache_DeveRetornarDoCache()
    {
        // Arrange
        var contasCache = new List<ContaPagarDto>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(), Descricao = "Conta teste 1" },
            new() { Id = ObjectId.GenerateNewId().ToString(), Descricao = "Conta teste 2" }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<List<ContaPagarDto>>("gp:contas-pagar:all"))
            .ReturnsAsync(contasCache);

        // Act
        var resultado = await _service.GetAllContasPagarAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().BeEquivalentTo(contasCache);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllContasPagarAsync_SemCache_DeveBuscarNoBanco()
    {
        // Arrange
        var contasBanco = new List<ContaPagar>
        {
            new() 
            { 
                Id = ObjectId.GenerateNewId().ToString(), 
                Descricao = "Conta teste 1",
                ValorOriginal = 1000.00M,
                Status = StatusContaPagar.Pendente,
                DataVencimento = DateTime.UtcNow.AddDays(30),
                Categoria = CategoriaConta.Fornecedores
            }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<List<ContaPagarDto>>("gp:contas-pagar:all"))
            .ReturnsAsync((List<ContaPagarDto>?)null);

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contasBanco);

        // Act
        var resultado = await _service.GetAllContasPagarAsync();

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Descricao.Should().Be("Conta teste 1");
        _cacheServiceMock.Verify(c => c.SetAsync("gp:contas-pagar:all", It.IsAny<List<ContaPagarDto>>(), TimeSpan.FromMinutes(2)), Times.Once);
    }

    [Fact]
    public async Task GetContaPagarByIdAsync_ContaExistente_DeveRetornarConta()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Descricao = "Conta teste",
            ValorOriginal = 1500.00M,
            Status = StatusContaPagar.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Categoria = CategoriaConta.Fornecedores
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        // Act
        var resultado = await _service.GetContaPagarByIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(id);
        resultado.Descricao.Should().Be("Conta teste");
        resultado.ValorOriginal.Should().Be(1500.00M);
    }

    [Fact]
    public async Task GetContaPagarByIdAsync_ContaInexistente_DeveRetornarNull()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((ContaPagar?)null);

        // Act
        var resultado = await _service.GetContaPagarByIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CreateContaPagarAsync_ComFornecedor_DeveCriarComNomeFornecedor()
    {
        // Arrange
        var fornecedorId = ObjectId.GenerateNewId().ToString();
        var fornecedor = new Fornecedor
        {
            Id = fornecedorId,
            RazaoSocial = "Fornecedor ABC",
            NomeFantasia = "Fornecedor ABC"
        };

        var dto = new CreateContaPagarDto
        {
            Descricao = "Nova conta",
            FornecedorId = fornecedorId,
            ValorOriginal = 1000.00M,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Categoria = CategoriaConta.Fornecedores
        };

        _fornecedorRepositoryMock.Setup(f => f.GetByIdAsync(fornecedorId))
            .ReturnsAsync(fornecedor);

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ContaPagar>()))
            .ReturnsAsync((ContaPagar conta) => conta);

        // Act
        var resultado = await _service.CreateContaPagarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Descricao.Should().Be("Nova conta");
        resultado.FornecedorNome.Should().Be("Fornecedor ABC");
        resultado.ValorOriginal.Should().Be(1000.00M);
        resultado.Status.Should().Be("1"); // StatusContaPagar.Pendente

        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemovePatternAsync("gp:contas-pagar:*"), Times.Once);
    }

    [Fact]
    public async Task UpdateContaPagarAsync_ContaExistente_DeveAtualizar()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Descricao = "Conta original",
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Categoria = CategoriaConta.Fornecedores
        };

        var dto = new UpdateContaPagarDto
        {
            Descricao = "Conta atualizada",
            ValorOriginal = 1200.00M,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(45),
            Categoria = CategoriaConta.Outros
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ContaPagar>()))
            .ReturnsAsync((ContaPagar conta) => conta);

        // Act
        var resultado = await _service.UpdateContaPagarAsync(id, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Descricao.Should().Be("Conta atualizada");
        resultado.ValorOriginal.Should().Be(1200.00M);
        resultado.Categoria.Should().Be("99"); // CategoriaConta.Outros

        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
    }

    [Fact]
    public async Task UpdateContaPagarAsync_ContaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Status = StatusContaPagar.Paga
        };

        var dto = new UpdateContaPagarDto
        {
            Descricao = "Tentativa de atualização"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        // Act & Assert
        var act = async () => await _service.UpdateContaPagarAsync(id, dto);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Não é possível alterar conta já paga");
    }

    [Fact]
    public async Task PagarContaAsync_ContaExistente_DeveProcessarPagamento()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Descricao = "Conta para pagamento",
            ValorOriginal = 1000.00M,
            Status = StatusContaPagar.Pendente,
            DataVencimento = DateTime.UtcNow.AddDays(30)
        };

        var dto = new PagarContaDto
        {
            Valor = 1000.00M,
            FormaPagamento = FormaPagamento.PIX,
            DataPagamento = DateTime.UtcNow,
            Observacoes = "Pagamento via PIX"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ContaPagar>()))
            .ReturnsAsync((ContaPagar conta) => conta);

        // Act
        var resultado = await _service.PagarContaAsync(id, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Status.Should().Be("2"); // StatusContaPagar.Paga
        resultado.ValorPago.Should().Be(1000.00M);
        resultado.FormaPagamento.Should().Be("PIX");

        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
    }

    [Fact]
    public async Task DeleteContaPagarAsync_ContaPendente_DeveExcluir()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Status = StatusContaPagar.Pendente
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        _repositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.DeleteContaPagarAsync(id);

        // Assert
        resultado.Should().BeTrue();
        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
    }

    [Fact]
    public async Task DeleteContaPagarAsync_ContaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Status = StatusContaPagar.Paga
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        // Act & Assert
        var act = async () => await _service.DeleteContaPagarAsync(id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Não é possível excluir conta já paga");
    }

    [Fact]
    public async Task CancelarContaAsync_ContaPendente_DeveCancelar()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagar
        {
            Id = id,
            Status = StatusContaPagar.Pendente
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(conta);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ContaPagar>()))
            .ReturnsAsync((ContaPagar conta) => conta);

        // Act
        var resultado = await _service.CancelarContaAsync(id);

        // Assert
        resultado.Should().BeTrue();
        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
    }

    [Fact]
    public async Task GetContasPagarByStatusAsync_DeveRetornarContasComStatusCorreto()
    {
        // Arrange
        var status = StatusContaPagar.Vencida;
        var contas = new List<ContaPagar>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = status,
                Descricao = "Conta vencida 1",
                ValorOriginal = 1000.00M,
                DataVencimento = DateTime.UtcNow.AddDays(-5),
                Categoria = CategoriaConta.Fornecedores
            }
        };

        _repositoryMock.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(contas);

        // Act
        var resultado = await _service.GetContasPagarByStatusAsync(status);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Status.Should().Be("4"); // StatusContaPagar.Vencida
        resultado.First().Descricao.Should().Be("Conta vencida 1");
    }

    [Fact]
    public async Task GetTotalPagarPorPeriodoAsync_DeveRetornarTotalCorreto()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 1);
        var fim = new DateTime(2024, 1, 31);
        var total = 5000.00M;

        _repositoryMock.Setup(r => r.GetTotalPagarPorPeriodoAsync(inicio, fim))
            .ReturnsAsync(total);

        // Act
        var resultado = await _service.GetTotalPagarPorPeriodoAsync(inicio, fim);

        // Assert
        resultado.Should().Be(total);
    }

    [Fact]
    public async Task AtualizarStatusContasAsync_DeveAtualizarContasPendentesEParciais()
    {
        // Arrange
        var contasPendentes = new List<ContaPagar>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = StatusContaPagar.Pendente,
                DataVencimento = DateTime.UtcNow.AddDays(-1) // Vencida
            }
        };

        var contasParciais = new List<ContaPagar>
        {
            new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = StatusContaPagar.PagamentoParcial,
                DataVencimento = DateTime.UtcNow.AddDays(-2) // Vencida
            }
        };

        _repositoryMock.Setup(r => r.GetByStatusAsync(StatusContaPagar.Pendente))
            .ReturnsAsync(contasPendentes);

        _repositoryMock.Setup(r => r.GetByStatusAsync(StatusContaPagar.PagamentoParcial))
            .ReturnsAsync(contasParciais);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ContaPagar>()))
            .ReturnsAsync((ContaPagar conta) => conta);

        // Act
        await _service.AtualizarStatusContasAsync();

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ContaPagar>()), Times.Exactly(2));
        _cacheServiceMock.Verify(c => c.RemoveAsync("gp:contas-pagar:all"), Times.Once);
    }
}
