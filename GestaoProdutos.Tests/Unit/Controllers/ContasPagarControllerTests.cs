using FluentAssertions;
using GestaoProdutos.API.Controllers;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace GestaoProdutos.Tests.Unit.Controllers;

/// <summary>
/// Testes unitários para o ContasPagarController
/// </summary>
public class ContasPagarControllerTests
{
    private readonly Mock<IContaPagarService> _serviceMock;
    private readonly Mock<ILogger<ContasPagarController>> _loggerMock;
    private readonly ContasPagarController _controller;

    public ContasPagarControllerTests()
    {
        _serviceMock = new Mock<IContaPagarService>();
        _loggerMock = new Mock<ILogger<ContasPagarController>>();
        _controller = new ContasPagarController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ComSucesso_DeveRetornarOkComLista()
    {
        // Arrange
        var contas = new List<ContaPagarDto>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(), Descricao = "Conta 1" },
            new() { Id = ObjectId.GenerateNewId().ToString(), Descricao = "Conta 2" }
        };

        _serviceMock.Setup(s => s.GetAllContasPagarAsync())
            .ReturnsAsync(contas);

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var contasRetornadas = okResult.Value.Should().BeAssignableTo<IEnumerable<ContaPagarDto>>().Subject;
        contasRetornadas.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ComExcecao_DeveRetornarInternalServerError()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllContasPagarAsync())
            .ThrowsAsync(new Exception("Erro no banco de dados"));

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var statusResult = resultado.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_ContaExistente_DeveRetornarOkComConta()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var conta = new ContaPagarDto
        {
            Id = id,
            Descricao = "Conta teste",
            ValorOriginal = 1000.00M
        };

        _serviceMock.Setup(s => s.GetContaPagarByIdAsync(id))
            .ReturnsAsync(conta);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var contaRetornada = okResult.Value.Should().BeOfType<ContaPagarDto>().Subject;
        contaRetornada.Id.Should().Be(id);
        contaRetornada.Descricao.Should().Be("Conta teste");
    }

    [Fact]
    public async Task GetById_ContaInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        _serviceMock.Setup(s => s.GetContaPagarByIdAsync(id))
            .ReturnsAsync((ContaPagarDto?)null);

        // Act
        var resultado = await _controller.GetById(id);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByStatus_ComSucesso_DeveRetornarOkComContasFiltradas()
    {
        // Arrange
        var status = StatusContaPagar.Vencida;
        var contas = new List<ContaPagarDto>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(), Status = "1" } // Vencida
        };

        _serviceMock.Setup(s => s.GetContasPagarByStatusAsync(status))
            .ReturnsAsync(contas);

        // Act
        var resultado = await _controller.GetByStatus(status);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var contasRetornadas = okResult.Value.Should().BeAssignableTo<IEnumerable<ContaPagarDto>>().Subject;
        contasRetornadas.Should().HaveCount(1);
        contasRetornadas.First().Status.Should().Be("1");
    }

    [Fact]
    public async Task GetVencendoEm_DiasValidos_DeveRetornarOkComContas()
    {
        // Arrange
        var dias = 7;
        var contas = new List<ContaPagarDto>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(), DataVencimento = DateTime.UtcNow.AddDays(dias) }
        };

        _serviceMock.Setup(s => s.GetContasPagarVencendoEmAsync(dias))
            .ReturnsAsync(contas);

        // Act
        var resultado = await _controller.GetVencendoEm(dias);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<ContaPagarDto>>();
    }

    [Fact]
    public async Task GetByPeriodo_DataValida_DeveRetornarOkComContas()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 1);
        var fim = new DateTime(2024, 1, 31);
        var contas = new List<ContaPagarDto>
        {
            new() { Id = ObjectId.GenerateNewId().ToString(), DataVencimento = new DateTime(2024, 1, 15) }
        };

        _serviceMock.Setup(s => s.GetContasPagarByPeriodoAsync(inicio, fim))
            .ReturnsAsync(contas);

        // Act
        var resultado = await _controller.GetByPeriodo(inicio, fim);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<ContaPagarDto>>();
    }

    [Fact]
    public async Task GetByPeriodo_DataInicioMaiorQueFim_DeveRetornarBadRequest()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 31);
        var fim = new DateTime(2024, 1, 1);

        // Act
        var resultado = await _controller.GetByPeriodo(inicio, fim);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var dto = new CreateContaPagarDto
        {
            Descricao = "Nova conta",
            ValorOriginal = 1000.00M,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = DateTime.UtcNow.AddDays(30),
            Categoria = CategoriaConta.Fornecedores
        };

        var contaCriada = new ContaPagarDto
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Descricao = dto.Descricao,
            ValorOriginal = dto.ValorOriginal
        };

        _serviceMock.Setup(s => s.CreateContaPagarAsync(dto))
            .ReturnsAsync(contaCriada);

        // Act
        var resultado = await _controller.Create(dto);

        // Assert
        var createdResult = resultado.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ContasPagarController.GetById));
        createdResult.Value.Should().BeOfType<ContaPagarDto>();
    }

    [Fact]
    public async Task Update_ContaExistente_DeveRetornarOkComContaAtualizada()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var dto = new UpdateContaPagarDto
        {
            Descricao = "Conta atualizada",
            ValorOriginal = 1200.00M
        };

        var contaAtualizada = new ContaPagarDto
        {
            Id = id,
            Descricao = dto.Descricao,
            ValorOriginal = dto.ValorOriginal
        };

        _serviceMock.Setup(s => s.UpdateContaPagarAsync(id, dto))
            .ReturnsAsync(contaAtualizada);

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ContaPagarDto>();
    }

    [Fact]
    public async Task Update_OperacaoInvalida_DeveRetornarBadRequest()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var dto = new UpdateContaPagarDto
        {
            Descricao = "Tentativa de atualização"
        };

        _serviceMock.Setup(s => s.UpdateContaPagarAsync(id, dto))
            .ThrowsAsync(new InvalidOperationException("Não é possível alterar conta já paga"));

        // Act
        var resultado = await _controller.Update(id, dto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ContaExistente_DeveRetornarNoContent()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        _serviceMock.Setup(s => s.DeleteContaPagarAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.Delete(id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ContaInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        _serviceMock.Setup(s => s.DeleteContaPagarAsync(id))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.Delete(id);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Pagar_DadosValidos_DeveRetornarOkComContaPaga()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        var dto = new PagarContaDto
        {
            Valor = 1000.00M,
            FormaPagamento = FormaPagamento.PIX,
            DataPagamento = DateTime.UtcNow
        };

        var contaPaga = new ContaPagarDto
        {
            Id = id,
            Status = "2", // Paga
            ValorPago = dto.Valor
        };

        _serviceMock.Setup(s => s.PagarContaAsync(id, dto))
            .ReturnsAsync(contaPaga);

        // Act
        var resultado = await _controller.Pagar(id, dto);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var contaRetornada = okResult.Value.Should().BeOfType<ContaPagarDto>().Subject;
        contaRetornada.Status.Should().Be("2");
        contaRetornada.ValorPago.Should().Be(1000.00M);
    }

    [Fact]
    public async Task Cancelar_ContaExistente_DeveRetornarOk()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();

        _serviceMock.Setup(s => s.CancelarContaAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.Cancelar(id);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetTotalPagar_ParametrosValidos_DeveRetornarOkComTotal()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 1);
        var fim = new DateTime(2024, 1, 31);
        var total = 10000.00M;

        _serviceMock.Setup(s => s.GetTotalPagarPorPeriodoAsync(inicio, fim))
            .ReturnsAsync(total);

        // Act
        var resultado = await _controller.GetTotalPagar(inicio, fim);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<object>().Subject;
        
        // Verificar se a resposta contém o total
        var totalProperty = response.GetType().GetProperty("total");
        totalProperty.Should().NotBeNull();
        totalProperty!.GetValue(response).Should().Be(total);
    }

    [Fact]
    public async Task GetQuantidadeVencidas_ComSucesso_DeveRetornarOkComQuantidade()
    {
        // Arrange
        var quantidade = 5;

        _serviceMock.Setup(s => s.GetQuantidadeContasVencidasAsync())
            .ReturnsAsync(quantidade);

        // Act
        var resultado = await _controller.GetQuantidadeVencidas();

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<object>().Subject;
        
        // Verificar se a resposta contém a quantidade
        var quantidadeProperty = response.GetType().GetProperty("quantidade");
        quantidadeProperty.Should().NotBeNull();
        quantidadeProperty!.GetValue(response).Should().Be(quantidade);
    }

    [Fact]
    public async Task AtualizarStatus_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.AtualizarStatusContasAsync())
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.AtualizarStatus();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ProcessarRecorrentes_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.ProcessarContasRecorrentesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.ProcessarRecorrentes();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }
}
