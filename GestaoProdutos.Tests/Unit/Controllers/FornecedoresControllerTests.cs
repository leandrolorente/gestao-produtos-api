using FluentAssertions;
using GestaoProdutos.API.Controllers;
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GestaoProdutos.Tests.Unit.Controllers;

public class FornecedoresControllerTests
{
    private readonly Mock<IFornecedorService> _fornecedorServiceMock;
    private readonly Mock<ILogger<FornecedoresController>> _loggerMock;
    private readonly FornecedoresController _controller;

    public FornecedoresControllerTests()
    {
        _fornecedorServiceMock = new Mock<IFornecedorService>();
        _loggerMock = new Mock<ILogger<FornecedoresController>>();
        _controller = new FornecedoresController(_fornecedorServiceMock.Object, _loggerMock.Object);
    }

    private static FornecedorDto CriarFornecedorDto()
    {
        return new FornecedorDto
        {
            Id = "123456789012345678901234",
            RazaoSocial = "Fornecedor Teste LTDA",
            NomeFantasia = "Fornecedor Teste",
            CnpjCpf = "12.345.678/0001-90",
            Email = "contato@fornecedorteste.com",
            Telefone = "(11) 99999-9999",
            Tipo = "Nacional",
            Status = "Ativo",
            PrazoPagamentoPadrao = 30,
            LimiteCredito = 50000m,
            TotalComprado = 25000m,
            QuantidadeCompras = 5,
            EhFrequente = true,
            Ativo = true
        };
    }

    [Fact]
    public async Task GetAllFornecedores_ComSucesso_DeveRetornarOkComLista()
    {
        // Arrange
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetAllFornecedoresAsync())
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetAllFornecedores();

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllFornecedores_ComExcecao_DeveRetornarInternalServerError()
    {
        // Arrange
        _fornecedorServiceMock.Setup(x => x.GetAllFornecedoresAsync())
            .ThrowsAsync(new Exception("Erro de teste"));

        // Act
        var resultado = await _controller.GetAllFornecedores();

        // Assert
        var statusResult = resultado.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetFornecedorById_ComIdExistente_DeveRetornarOkComFornecedor()
    {
        // Arrange
        var id = "123456789012345678901234";
        var fornecedor = CriarFornecedorDto();
        _fornecedorServiceMock.Setup(x => x.GetFornecedorByIdAsync(id))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _controller.GetFornecedorById(id);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedorRetornado = okResult.Value.Should().BeOfType<FornecedorDto>().Subject;
        fornecedorRetornado.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetFornecedorById_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.GetFornecedorByIdAsync(id))
            .ReturnsAsync((FornecedorDto?)null);

        // Act
        var resultado = await _controller.GetFornecedorById(id);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetFornecedorByCnpjCpf_ComCnpjExistente_DeveRetornarOkComFornecedor()
    {
        // Arrange
        var cnpjCpf = "12.345.678/0001-90";
        var fornecedor = CriarFornecedorDto();
        _fornecedorServiceMock.Setup(x => x.GetFornecedorByCnpjCpfAsync(cnpjCpf))
            .ReturnsAsync(fornecedor);

        // Act
        var resultado = await _controller.GetFornecedorByCnpjCpf(cnpjCpf);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedorRetornado = okResult.Value.Should().BeOfType<FornecedorDto>().Subject;
        fornecedorRetornado.CnpjCpf.Should().Be(cnpjCpf);
    }

    [Fact]
    public async Task BuscarFornecedores_ComTermo_DeveRetornarOkComLista()
    {
        // Arrange
        var termo = "Teste";
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.BuscarFornecedoresAsync(termo))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.BuscarFornecedores(termo);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFornecedoresPorTipo_ComTipoValido_DeveRetornarOkComLista()
    {
        // Arrange
        var tipo = TipoFornecedor.Nacional;
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetFornecedoresAtivosPorTipoAsync(tipo))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetFornecedoresPorTipo(tipo);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFornecedoresPorStatus_ComStatusValido_DeveRetornarOkComLista()
    {
        // Arrange
        var status = StatusFornecedor.Ativo;
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetFornecedoresPorStatusAsync(status))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetFornecedoresPorStatus(status);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateFornecedor_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var dto = new CreateFornecedorDto
        {
            RazaoSocial = "Novo Fornecedor LTDA",
            NomeFantasia = "Novo Fornecedor",
            CnpjCpf = "98.765.432/0001-10",
            Email = "novo@fornecedor.com",
            Tipo = TipoFornecedor.Nacional
        };

        var fornecedorCriado = CriarFornecedorDto();
        _fornecedorServiceMock.Setup(x => x.CreateFornecedorAsync(dto))
            .ReturnsAsync(fornecedorCriado);

        // Act
        var resultado = await _controller.CreateFornecedor(dto);

        // Assert
        var createdResult = resultado.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(FornecedoresController.GetFornecedorById));
        var fornecedorRetornado = createdResult.Value.Should().BeOfType<FornecedorDto>().Subject;
        fornecedorRetornado.Id.Should().Be(fornecedorCriado.Id);
    }

    [Fact]
    public async Task CreateFornecedor_ComCnpjDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        var dto = new CreateFornecedorDto
        {
            RazaoSocial = "Fornecedor Duplicado",
            CnpjCpf = "12.345.678/0001-90",
            Email = "duplicado@fornecedor.com"
        };

        _fornecedorServiceMock.Setup(x => x.CreateFornecedorAsync(dto))
            .ThrowsAsync(new InvalidOperationException("CNPJ já existe"));

        // Act
        var resultado = await _controller.CreateFornecedor(dto);

        // Assert
        resultado.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateFornecedor_ComDadosValidos_DeveRetornarOkComFornecedorAtualizado()
    {
        // Arrange
        var id = "123456789012345678901234";
        var dto = new UpdateFornecedorDto
        {
            RazaoSocial = "Fornecedor Atualizado LTDA",
            Email = "atualizado@fornecedor.com",
            Status = StatusFornecedor.Ativo
        };

        var fornecedorAtualizado = CriarFornecedorDto();
        _fornecedorServiceMock.Setup(x => x.UpdateFornecedorAsync(id, dto))
            .ReturnsAsync(fornecedorAtualizado);

        // Act
        var resultado = await _controller.UpdateFornecedor(id, dto);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedorRetornado = okResult.Value.Should().BeOfType<FornecedorDto>().Subject;
        fornecedorRetornado.Id.Should().Be(id);
    }

    [Fact]
    public async Task UpdateFornecedor_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = "123456789012345678901234";
        var dto = new UpdateFornecedorDto { RazaoSocial = "Teste" };

        _fornecedorServiceMock.Setup(x => x.UpdateFornecedorAsync(id, dto))
            .ThrowsAsync(new InvalidOperationException("Fornecedor não encontrado"));

        // Act
        var resultado = await _controller.UpdateFornecedor(id, dto);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteFornecedor_ComIdExistente_DeveRetornarNoContent()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.DeleteFornecedorAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.DeleteFornecedor(id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteFornecedor_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.DeleteFornecedorAsync(id))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.DeleteFornecedor(id);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task BloquearFornecedor_ComIdExistente_DeveRetornarOk()
    {
        // Arrange
        var id = "123456789012345678901234";
        var request = new BloquearFornecedorRequest { Motivo = "Inadimplência" };
        _fornecedorServiceMock.Setup(x => x.BloquearFornecedorAsync(id, request.Motivo))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.BloquearFornecedor(id, request);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DesbloquearFornecedor_ComIdExistente_DeveRetornarOk()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.DesbloquearFornecedorAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.DesbloquearFornecedor(id);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task InativarFornecedor_ComIdExistente_DeveRetornarOk()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.InativarFornecedorAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.InativarFornecedor(id);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AtivarFornecedor_ComIdExistente_DeveRetornarOk()
    {
        // Arrange
        var id = "123456789012345678901234";
        _fornecedorServiceMock.Setup(x => x.AtivarFornecedorAsync(id))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.AtivarFornecedor(id);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AdicionarProduto_ComIdsExistentes_DeveRetornarOk()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var produtoId = "produto123";
        _fornecedorServiceMock.Setup(x => x.AdicionarProdutoAsync(fornecedorId, produtoId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.AdicionarProduto(fornecedorId, produtoId);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoverProduto_ComIdsExistentes_DeveRetornarOk()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var produtoId = "produto123";
        _fornecedorServiceMock.Setup(x => x.RemoverProdutoAsync(fornecedorId, produtoId))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.RemoverProduto(fornecedorId, produtoId);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegistrarCompra_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var request = new RegistrarCompraRequest { Valor = 5000m };
        _fornecedorServiceMock.Setup(x => x.RegistrarCompraAsync(fornecedorId, request.Valor))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.RegistrarCompra(fornecedorId, request);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AtualizarCondicoesComerciais_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var request = new AtualizarCondicoesComerciaisRequest
        {
            PrazoPagamento = 45,
            LimiteCredito = 75000m,
            CondicoesPagamento = "2x sem juros"
        };

        _fornecedorServiceMock.Setup(x => x.AtualizarCondicoesComerciais(
            fornecedorId, request.PrazoPagamento, request.LimiteCredito, request.CondicoesPagamento))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.AtualizarCondicoesComerciais(fornecedorId, request);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AtualizarDadosBancarios_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var fornecedorId = "123456789012345678901234";
        var request = new AtualizarDadosBancariosRequest
        {
            Banco = "Banco do Brasil",
            Agencia = "1234-5",
            Conta = "12345-6",
            Pix = "fornecedor@teste.com"
        };

        _fornecedorServiceMock.Setup(x => x.AtualizarDadosBancarios(
            fornecedorId, request.Banco, request.Agencia, request.Conta, request.Pix))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.AtualizarDadosBancarios(fornecedorId, request);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFornecedoresComCompraRecente_ComParametroPadrao_DeveRetornarOkComLista()
    {
        // Arrange
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetFornecedoresComCompraRecenteAsync(90))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetFornecedoresComCompraRecente();

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFornecedoresFrequentes_DeveRetornarOkComLista()
    {
        // Arrange
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetFornecedoresFrequentesAsync())
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetFornecedoresFrequentes();

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFornecedoresPorProduto_ComProdutoId_DeveRetornarOkComLista()
    {
        // Arrange
        var produtoId = "produto123";
        var fornecedores = new List<FornecedorDto> { CriarFornecedorDto() };
        _fornecedorServiceMock.Setup(x => x.GetFornecedoresPorProdutoAsync(produtoId))
            .ReturnsAsync(fornecedores);

        // Act
        var resultado = await _controller.GetFornecedoresPorProduto(produtoId);

        // Assert
        var okResult = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var fornecedoresRetornados = okResult.Value.Should().BeAssignableTo<IEnumerable<FornecedorDto>>().Subject;
        fornecedoresRetornados.Should().HaveCount(1);
    }
}
