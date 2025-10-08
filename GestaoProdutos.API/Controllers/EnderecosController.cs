using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para gerenciamento de endereços
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnderecosController : ControllerBase
{
    private readonly IEnderecoService _enderecoService;

    public EnderecosController(IEnderecoService enderecoService)
    {
        _enderecoService = enderecoService;
    }

    /// <summary>
    /// Obtém todos os endereços ativos
    /// </summary>
    /// <returns>Lista de endereços</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> GetAllEnderecos()
    {
        try
        {
            var enderecos = await _enderecoService.GetAllAsync();
            return Ok(enderecos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um endereço específico por ID
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>Endereço encontrado</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EnderecoDto>> GetEnderecoById(string id)
    {
        try
        {
            var endereco = await _enderecoService.GetByIdAsync(id);
            
            if (endereco == null)
                return NotFound(new { message = "Endereço não encontrado" });

            return Ok(endereco);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Busca endereço por CEP
    /// </summary>
    /// <param name="cep">CEP para busca</param>
    /// <returns>Endereço encontrado</returns>
    [HttpGet("cep/{cep}")]
    public async Task<ActionResult<EnderecoDto>> GetEnderecoByCep(string cep)
    {
        try
        {
            var endereco = await _enderecoService.GetByCepAsync(cep);
            
            if (endereco == null)
                return NotFound(new { message = "Endereço com CEP informado não encontrado" });

            return Ok(endereco);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Busca endereços por cidade
    /// </summary>
    /// <param name="cidade">Nome da cidade</param>
    /// <returns>Lista de endereços da cidade</returns>
    [HttpGet("cidade/{cidade}")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> GetEnderecosByCidade(string cidade)
    {
        try
        {
            var enderecos = await _enderecoService.GetByCidadeAsync(cidade);
            return Ok(enderecos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Busca endereços por estado
    /// </summary>
    /// <param name="estado">Nome do estado ou UF</param>
    /// <returns>Lista de endereços do estado</returns>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> GetEnderecosByEstado(string estado)
    {
        try
        {
            var enderecos = await _enderecoService.GetByEstadoAsync(estado);
            return Ok(enderecos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Busca endereços por termo
    /// </summary>
    /// <param name="termo">Termo para busca (logradouro, bairro, cidade, etc.)</param>
    /// <returns>Lista de endereços encontrados</returns>
    [HttpGet("buscar/{termo}")]
    public async Task<ActionResult<IEnumerable<EnderecoDto>>> SearchEnderecos(string termo)
    {
        try
        {
            var enderecos = await _enderecoService.SearchAsync(termo);
            return Ok(enderecos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Cria um novo endereço
    /// </summary>
    /// <param name="createDto">Dados do endereço</param>
    /// <returns>Endereço criado</returns>
    [HttpPost]
    public async Task<ActionResult<EnderecoDto>> CreateEndereco([FromBody] CreateEnderecoDto createDto)
    {
        try
        {
            var endereco = await _enderecoService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetEnderecoById), new { id = endereco.Id }, endereco);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um endereço existente
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <param name="updateDto">Dados atualizados</param>
    /// <returns>Endereço atualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<EnderecoDto>> UpdateEndereco(string id, [FromBody] UpdateEnderecoDto updateDto)
    {
        try
        {
            var endereco = await _enderecoService.UpdateAsync(id, updateDto);
            
            if (endereco == null)
                return NotFound(new { message = "Endereço não encontrado" });

            return Ok(endereco);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um endereço (soft delete)
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEndereco(string id)
    {
        try
        {
            var success = await _enderecoService.DeleteAsync(id);
            
            if (!success)
                return NotFound(new { message = "Endereço não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Verifica se um endereço existe
    /// </summary>
    /// <param name="id">ID do endereço</param>
    /// <returns>True se existe, False caso contrário</returns>
    [HttpHead("{id}")]
    public async Task<ActionResult> CheckEnderecoExists(string id)
    {
        try
        {
            var exists = await _enderecoService.ExistsAsync(id);
            return exists ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}
