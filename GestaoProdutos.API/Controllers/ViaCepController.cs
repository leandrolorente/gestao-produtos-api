using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

/// <summary>
/// Controller para integração com ViaCEP
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ViaCepController : ControllerBase
{
    private readonly IViaCepService _viaCepService;
    private readonly ILogger<ViaCepController> _logger;

    public ViaCepController(IViaCepService viaCepService, ILogger<ViaCepController> logger)
    {
        _viaCepService = viaCepService;
        _logger = logger;
    }

    /// <summary>
    /// Busca informações de endereço pelo CEP usando a API do ViaCEP
    /// </summary>
    /// <param name="cep">CEP para consulta (formato: 12345678 ou 12345-678)</param>
    /// <returns>Dados do endereço encontrado</returns>
    /// <response code="200">Endereço encontrado com sucesso</response>
    /// <response code="400">CEP inválido</response>
    /// <response code="404">CEP não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{cep}")]
    [ProducesResponseType(typeof(ViaCepResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ViaCepResponseDto>> BuscarEnderecoPorCep(string cep)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cep))
            {
                return BadRequest(new { message = "CEP é obrigatório" });
            }

            var endereco = await _viaCepService.BuscarEnderecoPorCepAsync(cep);
            
            if (endereco == null)
            {
                return NotFound(new { message = "CEP não encontrado" });
            }

            return Ok(endereco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar CEP: {Cep}", cep);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Valida se o CEP possui formato correto
    /// </summary>
    /// <param name="cep">CEP para validar</param>
    /// <returns>Resultado da validação</returns>
    /// <response code="200">Resultado da validação</response>
    [HttpGet("validar/{cep}")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult ValidarCep(string cep)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cep))
            {
                return Ok(new { valido = false, message = "CEP não pode ser vazio" });
            }

            // Remove caracteres não numéricos
            var cepLimpo = System.Text.RegularExpressions.Regex.Replace(cep, @"[^\d]", "");
            
            // Valida se tem 8 dígitos
            var valido = cepLimpo.Length == 8 && cepLimpo.All(char.IsDigit);
            
            return Ok(new 
            { 
                valido, 
                cepFormatado = valido ? FormatarCep(cepLimpo) : null,
                message = valido ? "CEP válido" : "CEP deve conter exatamente 8 dígitos numéricos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CEP: {Cep}", cep);
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Formata CEP no padrão 12345-678
    /// </summary>
    /// <param name="cep">CEP limpo (8 dígitos)</param>
    /// <returns>CEP formatado</returns>
    private static string FormatarCep(string cep)
    {
        if (cep.Length != 8) return cep;
        return $"{cep.Substring(0, 5)}-{cep.Substring(5, 3)}";
    }
}