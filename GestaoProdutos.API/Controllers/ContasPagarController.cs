using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para gestão de contas a pagar
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContasPagarController : ControllerBase
{
    private readonly IContaPagarService _contaPagarService;
    private readonly ILogger<ContasPagarController> _logger;

    public ContasPagarController(
        IContaPagarService contaPagarService,
        ILogger<ContasPagarController> logger)
    {
        _contaPagarService = contaPagarService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna todas as contas a pagar
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetAll()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var contas = await _contaPagarService.GetAllContasPagarAsync();
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            if (elapsed < 50)
                Console.WriteLine($"🚀 [CACHE HIT] Contas a pagar retornadas em {elapsed:F2}ms");
            else
                Console.WriteLine($"🗄️ [DATABASE] Contas a pagar carregadas em {elapsed:F2}ms");

            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna uma conta a pagar específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContaPagarDto>> GetById(string id)
    {
        try
        {
            var conta = await _contaPagarService.GetContaPagarByIdAsync(id);
            
            if (conta == null)
                return NotFound(new { message = "Conta a pagar não encontrada" });

            return Ok(conta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conta a pagar por ID: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar por status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetByStatus(StatusContaPagar status)
    {
        try
        {
            var contas = await _contaPagarService.GetContasPagarByStatusAsync(status);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por status: {Status}", status);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar vencidas
    /// </summary>
    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetVencidas()
    {
        try
        {
            var contas = await _contaPagarService.GetContasPagarVencidasAsync();
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar vencidas");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar vencendo em X dias
    /// </summary>
    [HttpGet("vencendo/{dias}")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetVencendoEm([Range(1, 365)] int dias)
    {
        try
        {
            var contas = await _contaPagarService.GetContasPagarVencendoEmAsync(dias);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar vencendo em {Dias} dias", dias);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar por fornecedor
    /// </summary>
    [HttpGet("fornecedor/{fornecedorId}")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetByFornecedor(string fornecedorId)
    {
        try
        {
            var contas = await _contaPagarService.GetContasPagarByFornecedorAsync(fornecedorId);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por fornecedor: {FornecedorId}", fornecedorId);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar por período
    /// </summary>
    [HttpGet("periodo")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetByPeriodo(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var contas = await _contaPagarService.GetContasPagarByPeriodoAsync(inicio, fim);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por período: {Inicio} - {Fim}", inicio, fim);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna contas a pagar por categoria
    /// </summary>
    [HttpGet("categoria/{categoria}")]
    public async Task<ActionResult<IEnumerable<ContaPagarDto>>> GetByCategoria(CategoriaConta categoria)
    {
        try
        {
            var contas = await _contaPagarService.GetContasPagarByCategoriaAsync(categoria);
            return Ok(contas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por categoria: {Categoria}", categoria);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova conta a pagar
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContaPagarDto>> Create([FromBody] CreateContaPagarDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaPagarService.CreateContaPagarAsync(dto);
            
            _logger.LogInformation("Conta a pagar criada com sucesso: {Id}", conta.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = conta.Id }, conta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta a pagar");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza uma conta a pagar
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ContaPagarDto>> Update(string id, [FromBody] UpdateContaPagarDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaPagarService.UpdateContaPagarAsync(id, dto);
            
            if (conta == null)
                return NotFound(new { message = "Conta a pagar não encontrada" });

            _logger.LogInformation("Conta a pagar atualizada com sucesso: {Id}", id);
            
            return Ok(conta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta a pagar: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma conta a pagar
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            var resultado = await _contaPagarService.DeleteContaPagarAsync(id);
            
            if (!resultado)
                return NotFound(new { message = "Conta a pagar não encontrada" });

            _logger.LogInformation("Conta a pagar excluída com sucesso: {Id}", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta a pagar: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Realiza o pagamento de uma conta
    /// </summary>
    [HttpPost("{id}/pagar")]
    public async Task<ActionResult<ContaPagarDto>> Pagar(string id, [FromBody] PagarContaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conta = await _contaPagarService.PagarContaAsync(id, dto);
            
            if (conta == null)
                return NotFound(new { message = "Conta a pagar não encontrada" });

            _logger.LogInformation("Pagamento realizado com sucesso: {Id} - Valor: {Valor}", id, dto.Valor);
            
            return Ok(conta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar pagamento da conta: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancela uma conta a pagar
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> Cancelar(string id)
    {
        try
        {
            var resultado = await _contaPagarService.CancelarContaAsync(id);
            
            if (!resultado)
                return NotFound(new { message = "Conta a pagar não encontrada" });

            _logger.LogInformation("Conta a pagar cancelada com sucesso: {Id}", id);
            
            return Ok(new { message = "Conta cancelada com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar conta a pagar: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o total a pagar por período
    /// </summary>
    [HttpGet("total-pagar")]
    public async Task<ActionResult<decimal>> GetTotalPagar(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var total = await _contaPagarService.GetTotalPagarPorPeriodoAsync(inicio, fim);
            return Ok(new { total, periodo = $"{inicio:yyyy-MM-dd} a {fim:yyyy-MM-dd}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total a pagar por período");
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna o total pago por período
    /// </summary>
    [HttpGet("total-pago")]
    public async Task<ActionResult<decimal>> GetTotalPago(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim)
    {
        try
        {
            if (inicio > fim)
                return BadRequest(new { message = "Data de início deve ser menor que data de fim" });

            var total = await _contaPagarService.GetTotalPagoPorPeriodoAsync(inicio, fim);
            return Ok(new { total, periodo = $"{inicio:yyyy-MM-dd} a {fim:yyyy-MM-dd}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total pago por período");
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
            var quantidade = await _contaPagarService.GetQuantidadeContasVencidasAsync();
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
            await _contaPagarService.AtualizarStatusContasAsync();
            
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
            await _contaPagarService.ProcessarContasRecorrentesAsync();
            
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
