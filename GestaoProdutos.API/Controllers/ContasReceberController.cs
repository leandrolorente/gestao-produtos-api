using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para gestão de contas a receber
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContasReceberController : ControllerBase
{
    private readonly IContaReceberService _contaReceberService;
    private readonly ILogger<ContasReceberController> _logger;

    public ContasReceberController(
        IContaReceberService contaReceberService,
        ILogger<ContasReceberController> logger)
    {
        _contaReceberService = contaReceberService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna todas as contas a receber
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetAll()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var contas = await _contaReceberService.GetAllContasReceberAsync();
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            if (elapsed < 50)
                Console.WriteLine($"🚀 [CACHE HIT] Contas a receber retornadas em {elapsed:F2}ms");
            else
                Console.WriteLine($"🗄️ [DATABASE] Contas a receber carregadas em {elapsed:F2}ms");

            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna uma conta a receber específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContaReceberDto>> GetById(string id)
    {
        try
        {
            var conta = await _contaReceberService.GetContaReceberByIdAsync(id);
            
            if (conta == null)
                return NotFound(new { message = "Conta a receber não encontrada" });

            return Ok(conta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conta a receber por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber por status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetByStatus(StatusContaReceber status)
    {
        try
        {
            var contas = await _contaReceberService.GetContasReceberByStatusAsync(status);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por status: {Status}", status);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber vencidas
    /// </summary>
    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetVencidas()
    {
        try
        {
            var contas = await _contaReceberService.GetContasReceberVencidasAsync();
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber vencidas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber vencendo em X dias
    /// </summary>
    [HttpGet("vencendo/{dias}")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetVencendoEm([Range(1, 365)] int dias)
    {
        try
        {
            var contas = await _contaReceberService.GetContasReceberVencendoEmAsync(dias);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber vencendo em {Dias} dias", dias);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber por cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetByCliente(string clienteId)
    {
        try
        {
            var contas = await _contaReceberService.GetContasReceberByClienteAsync(clienteId);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por cliente: {ClienteId}", clienteId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber por vendedor
    /// </summary>
    [HttpGet("vendedor/{vendedorId}")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetByVendedor(string vendedorId)
    {
        try
        {
            var contas = await _contaReceberService.GetContasReceberByVendedorAsync(vendedorId);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por vendedor: {VendedorId}", vendedorId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a receber por período
    /// </summary>
    [HttpGet("periodo")]
    public async Task<ActionResult<IEnumerable<ContaReceberDto>>> GetByPeriodo(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var contas = await _contaReceberService.GetContasReceberByPeriodoAsync(inicio, fim);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por período: {Inicio} - {Fim}", inicio, fim);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova conta a receber
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContaReceberDto>> Create([FromBody] CreateContaReceberDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaReceberService.CreateContaReceberAsync(dto);
            
            _logger.LogInformation("Conta a receber criada com sucesso: {Id}", conta.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = conta.Id }, conta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta a receber");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma conta a receber
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ContaReceberDto>> Update(string id, [FromBody] UpdateContaReceberDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaReceberService.UpdateContaReceberAsync(id, dto);
            
            if (conta == null)
                return NotFound(new { message = "Conta a receber não encontrada" });

            _logger.LogInformation("Conta a receber atualizada com sucesso: {Id}", id);
            
            return Ok(conta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta a receber: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma conta a receber
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var resultado = await _contaReceberService.DeleteContaReceberAsync(id);
            
            if (!resultado)
                return NotFound(new { message = "Conta a receber não encontrada" });

            _logger.LogInformation("Conta a receber excluída com sucesso: {Id}", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta a receber: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Realiza o recebimento de uma conta
    /// </summary>
    [HttpPost("{id}/receber")]
    public async Task<ActionResult<ContaReceberDto>> Receber(string id, [FromBody] ReceberContaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaReceberService.ReceberContaAsync(id, dto);
            
            if (conta == null)
                return NotFound(new { message = "Conta a receber não encontrada" });

            _logger.LogInformation("Recebimento realizado com sucesso: {Id} - Valor: {Valor}", id, dto.Valor);
            
            return Ok(conta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar recebimento da conta: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancela uma conta a receber
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> Cancelar(string id)
    {
        try
        {
            var resultado = await _contaReceberService.CancelarContaAsync(id);
            
            if (!resultado)
                return NotFound(new { message = "Conta a receber não encontrada" });

            _logger.LogInformation("Conta a receber cancelada com sucesso: {Id}", id);
            
            return Ok(new { message = "Conta cancelada com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar conta a receber: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o total a receber por período
    /// </summary>
    [HttpGet("total-receber")]
    public async Task<ActionResult<decimal>> GetTotalReceber(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var total = await _contaReceberService.GetTotalReceberPorPeriodoAsync(inicio, fim);
            return Ok(new { total, periodo = $"{inicio:yyyy-MM-dd} a {fim:yyyy-MM-dd}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total a receber por período");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o total recebido por período
    /// </summary>
    [HttpGet("total-recebido")]
    public async Task<ActionResult<decimal>> GetTotalRecebido(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var total = await _contaReceberService.GetTotalRecebidoPorPeriodoAsync(inicio, fim);
            return Ok(new { total, periodo = $"{inicio:yyyy-MM-dd} a {fim:yyyy-MM-dd}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total recebido por período");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna a quantidade de contas vencidas
    /// </summary>
    [HttpGet("quantidade-vencidas")]
    public async Task<ActionResult<int>> GetQuantidadeVencidas()
    {
        try
        {
            var quantidade = await _contaReceberService.GetQuantidadeContasVencidasAsync();
            return Ok(new { quantidade });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar contas vencidas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza o status de todas as contas pendentes
    /// </summary>
    [HttpPost("atualizar-status")]
    public async Task<ActionResult> AtualizarStatus()
    {
        try
        {
            await _contaReceberService.AtualizarStatusContasAsync();
            
            _logger.LogInformation("Status das contas atualizados com sucesso");
            
            return Ok(new { message = "Status das contas atualizados com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status das contas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Processa contas recorrentes (geralmente executado por job)
    /// </summary>
    [HttpPost("processar-recorrentes")]
    public async Task<ActionResult> ProcessarRecorrentes()
    {
        try
        {
            await _contaReceberService.ProcessarContasRecorrentesAsync();
            
            _logger.LogInformation("Contas recorrentes processadas com sucesso");
            
            return Ok(new { message = "Contas recorrentes processadas com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar contas recorrentes");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}
