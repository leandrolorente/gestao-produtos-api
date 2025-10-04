using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoProdutos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _logger = logger;
    }

    /// <summary>
    /// Obter todos os produtos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetAllProdutos()
    {
        try
        {
            _logger.LogInformation("🔍 [PRODUTOS] Requisição GetAllProdutos recebida - Verificando cache...");
            var startTime = DateTime.UtcNow;
            
            var produtos = await _produtoService.GetAllProdutosAsync();
            
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            if (duration.TotalMilliseconds < 50)
            {
                _logger.LogInformation("🚀 [CACHE HIT] Produtos retornados do REDIS em {Duration}ms", duration.TotalMilliseconds);
                Console.WriteLine($"🚀 [CACHE HIT] Produtos retornados do REDIS em {duration.TotalMilliseconds:F2}ms");
            }
            else
            {
                _logger.LogInformation("🗄️ [DATABASE] Produtos buscados no MONGODB em {Duration}ms", duration.TotalMilliseconds);
                Console.WriteLine($"🗄️ [DATABASE] Produtos buscados no MONGODB em {duration.TotalMilliseconds:F2}ms");
            }
            
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter produto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProdutoDto>> GetProdutoById(string id)
    {
        try
        {
            _logger.LogInformation("🔍 [PRODUTO ID] Requisição GetProdutoById({Id}) - Verificando cache...", id);
            var startTime = DateTime.UtcNow;
            
            var produto = await _produtoService.GetProdutoByIdAsync(id);
            
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });

            if (duration.TotalMilliseconds < 50)
            {
                _logger.LogInformation("🚀 [CACHE HIT] Produto ID:{Id} retornado do REDIS em {Duration}ms", id, duration.TotalMilliseconds);
                Console.WriteLine($"🚀 [CACHE HIT] Produto ID:{id} retornado do REDIS em {duration.TotalMilliseconds:F2}ms");
            }
            else
            {
                _logger.LogInformation("🗄️ [DATABASE] Produto ID:{Id} buscado no MONGODB em {Duration}ms", id, duration.TotalMilliseconds);
                Console.WriteLine($"🗄️ [DATABASE] Produto ID:{id} buscado no MONGODB em {duration.TotalMilliseconds:F2}ms");
            }

            return Ok(produto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter produto por SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProdutoDto>> GetProdutoBySku(string sku)
    {
        try
        {
            var produto = await _produtoService.GetProdutoBySkuAsync(sku);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });

            return Ok(produto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter produto por código de barras
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProdutoDto>> GetProdutoByBarcode(string barcode)
    {
        try
        {
            var produto = await _produtoService.GetProdutoByBarcodeAsync(barcode);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });

            return Ok(produto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Obter produtos com estoque baixo
    /// </summary>
    [HttpGet("estoque-baixo")]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosComEstoqueBaixo()
    {
        try
        {
            var produtos = await _produtoService.GetProdutosComEstoqueBaixoAsync();
            return Ok(produtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Criar novo produto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> CreateProduto([FromBody] CreateProdutoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = await _produtoService.CreateProdutoAsync(dto);
            return CreatedAtAction(nameof(GetProdutoById), new { id = produto.Id }, produto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar produto
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProdutoDto>> UpdateProduto(string id, [FromBody] UpdateProdutoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = await _produtoService.UpdateProdutoAsync(id, dto);
            return Ok(produto);
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
    /// Excluir produto (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduto(string id)
    {
        try
        {
            var result = await _produtoService.DeleteProdutoAsync(id);
            if (!result)
                return NotFound(new { message = "Produto não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar estoque de um produto
    /// </summary>
    [HttpPatch("{id}/estoque")]
    public async Task<ActionResult> UpdateEstoque(string id, [FromBody] int novaQuantidade)
    {
        try
        {
            var result = await _produtoService.UpdateEstoqueAsync(id, novaQuantidade);
            if (!result)
                return NotFound(new { message = "Produto não encontrado" });

            return Ok(new { message = "Estoque atualizado com sucesso", novaQuantidade });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
        }
    }
}