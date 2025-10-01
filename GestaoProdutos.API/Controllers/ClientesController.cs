using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Obter todos os clientes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAllClientes()
    {
        try
        {
            var clientes = await _clienteService.GetAllClientesAsync();
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter cliente por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> GetClienteById(string id)
    {
        try
        {
            var cliente = await _clienteService.GetClienteByIdAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente não encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter cliente por CPF/CNPJ
    /// </summary>
    [HttpGet("cpf-cnpj/{cpfCnpj}")]
    public async Task<ActionResult<ClienteDto>> GetClienteByCpfCnpj(string cpfCnpj)
    {
        try
        {
            var cliente = await _clienteService.GetClienteByCpfCnpjAsync(cpfCnpj);
            if (cliente == null)
                return NotFound(new { message = "Cliente não encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter clientes ativos por tipo (PF ou PJ)
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientesPorTipo(int tipo)
    {
        try
        {
            if (!Enum.IsDefined(typeof(TipoCliente), tipo))
                return BadRequest(new { message = "Tipo de cliente inválido" });

            var tipoCliente = (TipoCliente)tipo;
            var clientes = await _clienteService.GetClientesAtivosPorTipoAsync(tipoCliente);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter clientes com compra recente
    /// </summary>
    [HttpGet("compra-recente")]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientesComCompraRecente([FromQuery] int dias = 30)
    {
        try
        {
            var clientes = await _clienteService.GetClientesComCompraRecenteAsync(dias);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Criar novo cliente
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] CreateClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _clienteService.CreateClienteAsync(dto);
            return CreatedAtAction(nameof(GetClienteById), new { id = cliente.Id }, cliente);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar cliente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClienteDto>> UpdateCliente(string id, [FromBody] UpdateClienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _clienteService.UpdateClienteAsync(id, dto);
            return Ok(cliente);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Excluir cliente (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCliente(string id)
    {
        try
        {
            var result = await _clienteService.DeleteClienteAsync(id);
            if (!result)
                return NotFound(new { message = "Cliente não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Alternar status do cliente (ativo/inativo)
    /// </summary>
    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult> ToggleStatusCliente(string id)
    {
        try
        {
            var result = await _clienteService.ToggleStatusClienteAsync(id);
            if (!result)
                return NotFound(new { message = "Cliente não encontrado" });

            return Ok(new { message = "Status do cliente alterado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Registrar compra de um cliente
    /// </summary>
    [HttpPatch("{id}/registrar-compra")]
    public async Task<ActionResult> RegistrarCompra(string id)
    {
        try
        {
            var result = await _clienteService.RegistrarCompraAsync(id);
            if (!result)
                return NotFound(new { message = "Cliente não encontrado" });

            return Ok(new { message = "Compra registrada com sucesso", dataCompra = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}