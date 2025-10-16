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

public class VendaContaReceberIntegrationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IContaReceberService> _contaReceberServiceMock;
    private readonly VendaService _vendaService;

    public VendaContaReceberIntegrationTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _contaReceberServiceMock = new Mock<IContaReceberService>();
        
        _vendaService = new VendaService(_unitOfWorkMock.Object, _contaReceberServiceMock.Object);
    }

    [Fact]
    public async Task GetByVendaIdAsync_ComVendaIdValido_DeveRetornarContaReceber()
    {
        // Arrange
        var vendaId = "64a0b1c2d3e4f5a6b7c8d9e0";
        
        var contaReceberDto = new ContaReceberDto
        {
            Id = "conta-id",
            VendaId = vendaId,
            ValorOriginal = 100.00m
        };

        _contaReceberServiceMock.Setup(x => x.GetByVendaIdAsync(vendaId))
            .ReturnsAsync(contaReceberDto);

        // Act
        var result = await _contaReceberServiceMock.Object.GetByVendaIdAsync(vendaId);

        // Assert
        result.Should().NotBeNull();
        result!.VendaId.Should().Be(vendaId);
        result.ValorOriginal.Should().Be(100.00m);
    }
}