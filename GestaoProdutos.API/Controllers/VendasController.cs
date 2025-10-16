using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para gerenciamento de vendas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VendasController : ControllerBase
{
    private readonly IVendaService _vendaService;
    private readonly ILogger<VendasController> _logger;

    public VendasController(IVendaService vendaService, ILogger<VendasController> logger)
    {
        _vendaService = vendaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém todas as vendas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetAllVendas()
    {
        try
        {
            var vendas = await _vendaService.GetAllVendasAsync();
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma venda por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<VendaDto>> GetVendaById(string id)
    {
        try
        {
            var venda = await _vendaService.GetVendaByIdAsync(id);
            if (venda == null)
                return NotFound(new { message = "Venda não encontrada" });

            return Ok(venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma venda por número
    /// </summary>
    [HttpGet("numero/{numero}")]
    public async Task<ActionResult<VendaDto>> GetVendaByNumero(string numero)
    {
        try
        {
            var venda = await _vendaService.GetVendaByNumeroAsync(numero);
            if (venda == null)
                return NotFound(new { message = "Venda não encontrada" });

            return Ok(venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda por número: {Numero}", numero);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas por cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorCliente(string clienteId)
    {
        try
        {
            var vendas = await _vendaService.GetVendasPorClienteAsync(clienteId);
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por cliente: {ClienteId}", clienteId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas por vendedor
    /// </summary>
    [HttpGet("vendedor/{vendedorId}")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorVendedor(string vendedorId)
    {
        try
        {
            var vendas = await _vendaService.GetVendasPorVendedorAsync(vendedorId);
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por vendedor: {VendedorId}", vendedorId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas por status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorStatus(string status)
    {
        try
        {
            var vendas = await _vendaService.GetVendasPorStatusAsync(status);
            return Ok(vendas);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por status: {Status}", status);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas por período
    /// </summary>
    [HttpGet("periodo")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorPeriodo(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        try
        {
            if (dataInicio > dataFim)
                return BadRequest(new { message = "Data início deve ser menor que data fim" });

            var vendas = await _vendaService.GetVendasPorPeriodoAsync(dataInicio, dataFim);
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas por período: {DataInicio} - {DataFim}", dataInicio, dataFim);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas vencidas
    /// </summary>
    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasVencidas()
    {
        try
        {
            var vendas = await _vendaService.GetVendasVencidasAsync();
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas vencidas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém vendas de hoje
    /// </summary>
    [HttpGet("hoje")]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasHoje()
    {
        try
        {
            var vendas = await _vendaService.GetVendasHojeAsync();
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas de hoje");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova venda
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<VendaDto>> CreateVenda([FromBody] CreateVendaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obter ID do usuário logado como vendedor
            var vendedorId = HttpContext.User.FindFirst("id")?.Value;

            var venda = await _vendaService.CreateVendaAsync(dto, vendedorId);
            return CreatedAtAction(nameof(GetVendaById), new { id = venda.Id }, venda);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma venda existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<VendaDto>> UpdateVenda(string id, [FromBody] UpdateVendaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var venda = await _vendaService.UpdateVendaAsync(id, dto);
            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar venda: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove uma venda
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult> DeleteVenda(string id)
    {
        try
        {
            var resultado = await _vendaService.DeleteVendaAsync(id);
            if (!resultado)
                return NotFound(new { message = "Venda não encontrada" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir venda: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Confirma uma venda
    /// </summary>
    [HttpPatch("{id}/confirmar")]
    public async Task<ActionResult<VendaDto>> ConfirmarVenda(string id)
    {
        try
        {
            var venda = await _vendaService.ConfirmarVendaAsync(id);
            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar venda: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Finaliza uma venda
    /// </summary>
    [HttpPatch("{id}/finalizar")]
    public async Task<ActionResult<VendaDto>> FinalizarVenda(string id)
    {
        try
        {
            var venda = await _vendaService.FinalizarVendaAsync(id);
            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao finalizar venda: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancela uma venda
    /// </summary>
    [HttpPatch("{id}/cancelar")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<VendaDto>> CancelarVenda(string id)
    {
        try
        {
            var venda = await _vendaService.CancelarVendaAsync(id);
            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar venda: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém estatísticas de vendas
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<VendasStatsDto>> GetVendasStats()
    {
        try
        {
            var stats = await _vendaService.GetVendasStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas de vendas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém o próximo número de venda
    /// </summary>
    [HttpGet("proximo-numero")]
    public async Task<ActionResult<string>> GetProximoNumero()
    {
        try
        {
            var numero = await _vendaService.GetProximoNumeroVendaAsync();
            return Ok(new { numero });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar próximo número de venda");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}
