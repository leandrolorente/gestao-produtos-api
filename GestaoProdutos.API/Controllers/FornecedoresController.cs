using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para operações com fornecedores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    private readonly IFornecedorService _fornecedorService;
    private readonly ILogger<FornecedoresController> _logger;

    public FornecedoresController(
        IFornecedorService fornecedorService,
        ILogger<FornecedoresController> logger)
    {
        _fornecedorService = fornecedorService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todos os fornecedores
    /// </summary>
    /// <returns>Lista de fornecedores</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetAllFornecedores()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var fornecedores = await _fornecedorService.GetAllFornecedoresAsync();
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (duration < 50)
            {
                Console.WriteLine($"🚀 [CACHE HIT] Fornecedores retornados em {duration:F2}ms");
            }
            else
            {
                Console.WriteLine($"🗄️ [DATABASE] Fornecedores buscados no MongoDB em {duration:F2}ms");
            }

            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os fornecedores");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém lista simplificada de fornecedores
    /// </summary>
    /// <returns>Lista simplificada de fornecedores</returns>
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<FornecedorListDto>>> GetAllFornecedoresList()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetAllFornecedoresListAsync();
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lista de fornecedores");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um fornecedor por ID
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Dados do fornecedor</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FornecedorDto>> GetFornecedorById(string id)
    {
        try
        {
            var fornecedor = await _fornecedorService.GetFornecedorByIdAsync(id);
            
            if (fornecedor == null)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(fornecedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedor por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedor por CNPJ ou CPF
    /// </summary>
    /// <param name="cnpjCpf">CNPJ ou CPF do fornecedor</param>
    /// <returns>Dados do fornecedor</returns>
    [HttpGet("cnpj-cpf/{cnpjCpf}")]
    public async Task<ActionResult<FornecedorDto>> GetFornecedorByCnpjCpf(string cnpjCpf)
    {
        try
        {
            var fornecedor = await _fornecedorService.GetFornecedorByCnpjCpfAsync(cnpjCpf);
            
            if (fornecedor == null)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(fornecedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedor por CNPJ/CPF: {CnpjCpf}", cnpjCpf);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Busca fornecedores por termo
    /// </summary>
    /// <param name="termo">Termo de busca</param>
    /// <returns>Lista de fornecedores encontrados</returns>
    [HttpGet("buscar/{termo}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> BuscarFornecedores(string termo)
    {
        try
        {
            var fornecedores = await _fornecedorService.BuscarFornecedoresAsync(termo);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores com termo: {Termo}", termo);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedores por tipo
    /// </summary>
    /// <param name="tipo">Tipo do fornecedor (Nacional/Internacional)</param>
    /// <returns>Lista de fornecedores do tipo especificado</returns>
    [HttpGet("tipo/{tipo}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedoresPorTipo(TipoFornecedor tipo)
    {
        try
        {
            var fornecedores = await _fornecedorService.GetFornecedoresAtivosPorTipoAsync(tipo);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores por tipo: {Tipo}", tipo);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedores por status
    /// </summary>
    /// <param name="status">Status do fornecedor</param>
    /// <returns>Lista de fornecedores com o status especificado</returns>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedoresPorStatus(StatusFornecedor status)
    {
        try
        {
            var fornecedores = await _fornecedorService.GetFornecedoresPorStatusAsync(status);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores por status: {Status}", status);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedores com compras recentes
    /// </summary>
    /// <param name="dias">Número de dias para considerar como recente (padrão: 90)</param>
    /// <returns>Lista de fornecedores com compras recentes</returns>
    [HttpGet("compras-recentes")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedoresComCompraRecente([FromQuery] int dias = 90)
    {
        try
        {
            var fornecedores = await _fornecedorService.GetFornecedoresComCompraRecenteAsync(dias);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores com compras recentes");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedores frequentes
    /// </summary>
    /// <returns>Lista de fornecedores frequentes</returns>
    [HttpGet("frequentes")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedoresFrequentes()
    {
        try
        {
            var fornecedores = await _fornecedorService.GetFornecedoresFrequentesAsync();
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores frequentes");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém fornecedores de um produto específico
    /// </summary>
    /// <param name="produtoId">ID do produto</param>
    /// <returns>Lista de fornecedores do produto</returns>
    [HttpGet("produto/{produtoId}")]
    public async Task<ActionResult<IEnumerable<FornecedorDto>>> GetFornecedoresPorProduto(string produtoId)
    {
        try
        {
            var fornecedores = await _fornecedorService.GetFornecedoresPorProdutoAsync(produtoId);
            return Ok(fornecedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores por produto: {ProdutoId}", produtoId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo fornecedor
    /// </summary>
    /// <param name="dto">Dados do fornecedor a ser criado</param>
    /// <returns>Fornecedor criado</returns>
    [HttpPost]
    public async Task<ActionResult<FornecedorDto>> CreateFornecedor([FromBody] CreateFornecedorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fornecedor = await _fornecedorService.CreateFornecedorAsync(dto);
            return CreatedAtAction(nameof(GetFornecedorById), new { id = fornecedor.Id }, fornecedor);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fornecedor: {RazaoSocial}", dto.RazaoSocial);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um fornecedor existente
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="dto">Dados atualizados do fornecedor</param>
    /// <returns>Fornecedor atualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<FornecedorDto>> UpdateFornecedor(string id, [FromBody] UpdateFornecedorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fornecedor = await _fornecedorService.UpdateFornecedorAsync(id, dto);
            return Ok(fornecedor);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um fornecedor (soft delete)
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFornecedor(string id)
    {
        try
        {
            var sucesso = await _fornecedorService.DeleteFornecedorAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Bloqueia um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <param name="request">Dados da solicitação de bloqueio</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id}/bloquear")]
    public async Task<ActionResult> BloquearFornecedor(string id, [FromBody] BloquearFornecedorRequest request)
    {
        try
        {
            var sucesso = await _fornecedorService.BloquearFornecedorAsync(id, request.Motivo);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Fornecedor bloqueado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao bloquear fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Desbloqueia um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id}/desbloquear")]
    public async Task<ActionResult> DesbloquearFornecedor(string id)
    {
        try
        {
            var sucesso = await _fornecedorService.DesbloquearFornecedorAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Fornecedor desbloqueado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desbloquear fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Inativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id}/inativar")]
    public async Task<ActionResult> InativarFornecedor(string id)
    {
        try
        {
            var sucesso = await _fornecedorService.InativarFornecedorAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Fornecedor inativado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inativar fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Ativa um fornecedor
    /// </summary>
    /// <param name="id">ID do fornecedor</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{id}/ativar")]
    public async Task<ActionResult> AtivarFornecedor(string id)
    {
        try
        {
            var sucesso = await _fornecedorService.AtivarFornecedorAsync(id);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Fornecedor ativado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar fornecedor: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona um produto a um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="produtoId">ID do produto</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{fornecedorId}/produtos/{produtoId}")]
    public async Task<ActionResult> AdicionarProduto(string fornecedorId, string produtoId)
    {
        try
        {
            var sucesso = await _fornecedorService.AdicionarProdutoAsync(fornecedorId, produtoId);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Produto adicionado ao fornecedor com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar produto ao fornecedor: {FornecedorId}, {ProdutoId}", fornecedorId, produtoId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove um produto de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="produtoId">ID do produto</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{fornecedorId}/produtos/{produtoId}")]
    public async Task<ActionResult> RemoverProduto(string fornecedorId, string produtoId)
    {
        try
        {
            var sucesso = await _fornecedorService.RemoverProdutoAsync(fornecedorId, produtoId);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Produto removido do fornecedor com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produto do fornecedor: {FornecedorId}, {ProdutoId}", fornecedorId, produtoId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Registra uma compra para um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="request">Dados da compra</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("{fornecedorId}/compras")]
    public async Task<ActionResult> RegistrarCompra(string fornecedorId, [FromBody] RegistrarCompraRequest request)
    {
        try
        {
            var sucesso = await _fornecedorService.RegistrarCompraAsync(fornecedorId, request.Valor);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Compra registrada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar compra: {FornecedorId}, {Valor}", fornecedorId, request.Valor);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza condições comerciais de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="request">Novas condições comerciais</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{fornecedorId}/condicoes-comerciais")]
    public async Task<ActionResult> AtualizarCondicoesComerciais(string fornecedorId, [FromBody] AtualizarCondicoesComerciaisRequest request)
    {
        try
        {
            var sucesso = await _fornecedorService.AtualizarCondicoesComerciais(
                fornecedorId, 
                request.PrazoPagamento, 
                request.LimiteCredito, 
                request.CondicoesPagamento);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Condições comerciais atualizadas com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar condições comerciais: {FornecedorId}", fornecedorId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza dados bancários de um fornecedor
    /// </summary>
    /// <param name="fornecedorId">ID do fornecedor</param>
    /// <param name="request">Novos dados bancários</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{fornecedorId}/dados-bancarios")]
    public async Task<ActionResult> AtualizarDadosBancarios(string fornecedorId, [FromBody] AtualizarDadosBancariosRequest request)
    {
        try
        {
            var sucesso = await _fornecedorService.AtualizarDadosBancarios(
                fornecedorId, 
                request.Banco, 
                request.Agencia, 
                request.Conta, 
                request.Pix);
            
            if (!sucesso)
            {
                return NotFound(new { message = "Fornecedor não encontrado" });
            }

            return Ok(new { message = "Dados bancários atualizados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar dados bancários: {FornecedorId}", fornecedorId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}

// DTOs para requests específicos
public record BloquearFornecedorRequest
{
    public string? Motivo { get; init; }
}

public record RegistrarCompraRequest
{
    public decimal Valor { get; init; }
}

public record AtualizarCondicoesComerciaisRequest
{
    public int PrazoPagamento { get; init; }
    public decimal LimiteCredito { get; init; }
    public string? CondicoesPagamento { get; init; }
}

public record AtualizarDadosBancariosRequest
{
    public string? Banco { get; init; }
    public string? Agencia { get; init; }
    public string? Conta { get; init; }
    public string? Pix { get; init; }
}
