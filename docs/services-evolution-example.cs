// Alternativa: Evoluir Services com padrões mais robustos
using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Entities;

namespace GestaoProdutos.Application.Services;

// Evolução do Service atual - adicionar Result Pattern
public class ProdutoServiceEvoluido : IProdutoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProdutoServiceEvoluido> _logger;

    public async Task<Result<ProdutoDto>> CreateProdutoAsync(CreateProdutoDto dto)
    {
        try
        {
            // Validações de negócio centralizadas
            var validationResult = await ValidateCreateProduto(dto);
            if (!validationResult.IsSuccess)
                return Result<ProdutoDto>.Failure(validationResult.Error);

            // Lógica de criação
            var produto = new Produto
            {
                Nome = dto.Name,
                Sku = dto.Sku,
                Quantidade = dto.Quantity,
                Preco = dto.Price,
                Categoria = dto.Categoria,
                EstoqueMinimo = dto.EstoqueMinimo
            };

            await _unitOfWork.Produtos.CreateAsync(produto);
            await _unitOfWork.SaveChangesAsync();

            // Log estruturado
            _logger.LogInformation("Produto criado: {ProdutoId} - {Nome}", produto.Id, produto.Nome);

            return Result<ProdutoDto>.Success(MapToDto(produto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {@Dto}", dto);
            return Result<ProdutoDto>.Failure($"Erro interno: {ex.Message}");
        }
    }

    private async Task<Result> ValidateCreateProduto(CreateProdutoDto dto)
    {
        // Centralizar validações de negócio
        if (await _unitOfWork.Produtos.SkuJaExisteAsync(dto.Sku))
            return Result.Failure("SKU já existe");

        if (dto.Price <= 0)
            return Result.Failure("Preço deve ser maior que zero");

        // Outras validações...
        return Result.Success();
    }
}

// Result Pattern - simples e efetivo
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Error { get; protected set; } = string.Empty;

    protected Result(bool isSuccess, string error = "")
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value = default, string error = "") 
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value);
    public static new Result<T> Failure(string error) => new(false, default, error);
}

// Controller adaptado para Result Pattern
[ApiController]
[Route("api/[controller]")]
public class ProdutosControllerEvoluido : ControllerBase
{
    private readonly IProdutoService _produtoService;

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> CreateProduto([FromBody] CreateProdutoDto dto)
    {
        var result = await _produtoService.CreateProdutoAsync(dto);
        
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetProdutoById), 
            new { id = result.Value!.Id }, result.Value);
    }
}
