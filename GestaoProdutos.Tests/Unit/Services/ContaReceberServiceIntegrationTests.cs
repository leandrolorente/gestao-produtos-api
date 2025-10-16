using FluentAssertions;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace GestaoProdutos.Tests.Unit.Services;

public class ContaReceberServiceIntegrationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<ContaReceberService>> _loggerMock;
    private readonly ContaReceberService _contaReceberService;

    public ContaReceberServiceIntegrationTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ContaReceberService>>();
        _contaReceberService = new ContaReceberService(_unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByVendaIdAsync_ComVendaIdValido_DeveRetornarContaReceber()
    {
        // Arrange
        var vendaId = "64a0b1c2d3e4f5a6b7c8d9e0";
        var clienteId = "64a0b1c2d3e4f5a6b7c8d9e1";
        
        var contaReceber = new ContaReceber
        {
            Descricao = "Venda à prazo",
            ClienteId = clienteId,
            VendaId = vendaId,
            ValorOriginal = 100.00m,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Status = StatusContaReceber.Pendente
        };

        _unitOfWorkMock.Setup(x => x.ContasReceber.GetByVendaIdAsync(vendaId))
            .ReturnsAsync(contaReceber);

        // Act
        var result = await _contaReceberService.GetByVendaIdAsync(vendaId);

        // Assert
        result.Should().NotBeNull();
        result!.VendaId.Should().Be(vendaId);
        result.ClienteId.Should().Be(clienteId);
        result.ValorOriginal.Should().Be(100.00m);
    }

    [Fact]
    public async Task GetByVendaIdAsync_ComVendaIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var vendaId = "64a0b1c2d3e4f5a6b7c8d9e0";

        _unitOfWorkMock.Setup(x => x.ContasReceber.GetByVendaIdAsync(vendaId))
            .ReturnsAsync((ContaReceber?)null);

        // Act
        var result = await _contaReceberService.GetByVendaIdAsync(vendaId);

        // Assert
        result.Should().BeNull();
    }
}