// EXEMPLO: Como você poderia implementar Commands apenas onde faz sentido
// Mantenha Services para operações simples, use Commands para operações complexas

using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Domain.Entities;

namespace GestaoProdutos.Application.Commands;

// Command para operações complexas (exemplo: processar venda completa)
public record ProcessarVendaCommand
{
    public string ClienteId { get; init; } = string.Empty;
    public List<ItemVenda> Itens { get; init; } = new();
    public string FormaPagamento { get; init; } = string.Empty;
    public decimal Desconto { get; init; }
    public string? Observacoes { get; init; }
}

public record ItemVenda
{
    public string ProdutoId { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public decimal PrecoUnitario { get; init; }
}

// Handler apenas para operações que realmente precisam
public class ProcessarVendaCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IEstoqueService _estoqueService;

    public ProcessarVendaCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IEstoqueService estoqueService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _estoqueService = estoqueService;
    }

    public async Task<VendaDto> Handle(ProcessarVendaCommand command)
    {
        // 1. Validar cliente
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(command.ClienteId);
        if (cliente == null) throw new ArgumentException("Cliente não encontrado");

        // 2. Validar produtos e estoque
        var produtos = new List<Produto>();
        foreach (var item in command.Itens)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
            if (produto == null) throw new ArgumentException($"Produto {item.ProdutoId} não encontrado");
            if (produto.Quantidade < item.Quantidade) 
                throw new InvalidOperationException($"Estoque insuficiente para {produto.Nome}");
            produtos.Add(produto);
        }

        // 3. Calcular totais
        var subtotal = command.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        var total = subtotal - command.Desconto;

        // 4. Processar venda (Transaction)
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Criar venda
            var venda = new Venda
            {
                ClienteId = command.ClienteId,
                DataVenda = DateTime.UtcNow,
                Subtotal = subtotal,
                Desconto = command.Desconto,
                Total = total,
                FormaPagamento = command.FormaPagamento,
                Status = StatusVenda.Concluida
            };

            await _unitOfWork.Vendas.CreateAsync(venda);

            // Atualizar estoque
            foreach (var (item, produto) in command.Itens.Zip(produtos))
            {
                produto.AtualizarEstoque(produto.Quantidade - item.Quantidade);
                await _unitOfWork.Produtos.UpdateAsync(produto);
            }

            // Atualizar última compra do cliente
            cliente.AtualizarUltimaCompra();
            await _unitOfWork.Clientes.UpdateAsync(cliente);

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            // Side effects (não bloqueiam a resposta)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.EnviarComprovanteVenda(cliente.Email, venda);
                    await _estoqueService.VerificarEstoqueBaixo();
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the main operation
                    Console.WriteLine($"Erro em side effects: {ex.Message}");
                }
            });

            return MapToDto(venda);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static VendaDto MapToDto(Venda venda) => new()
    {
        Id = venda.Id,
        ClienteId = venda.ClienteId,
        DataVenda = venda.DataVenda,
        Total = venda.Total,
        Status = venda.Status.ToString()
    };
}

// MANTENHA Services para operações simples
public class ProdutoService : IProdutoService
{
    // Operações CRUD simples continuam como estão
    public async Task<ProdutoDto> CreateProdutoAsync(CreateProdutoDto dto)
    {
        // Lógica simples, não precisa de Command
        var produto = new Produto { /* ... */ };
        await _unitOfWork.Produtos.CreateAsync(produto);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(produto);
    }
}