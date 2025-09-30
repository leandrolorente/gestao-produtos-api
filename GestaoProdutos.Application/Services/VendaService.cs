using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using System.Globalization;

namespace GestaoProdutos.Application.Services;

public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork;

    public VendaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VendaDto>> GetAllVendasAsync()
    {
        var vendas = await _unitOfWork.Vendas.GetAllAsync();
        return vendas.Select(MapToDto);
    }

    public async Task<VendaDto?> GetVendaByIdAsync(string id)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        return venda != null ? MapToDto(venda) : null;
    }

    public async Task<VendaDto?> GetVendaByNumeroAsync(string numero)
    {
        var venda = await _unitOfWork.Vendas.GetVendaPorNumeroAsync(numero);
        return venda != null ? MapToDto(venda) : null;
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorClienteAsync(string clienteId)
    {
        var vendas = await _unitOfWork.Vendas.GetVendasPorClienteAsync(clienteId);
        return vendas.Select(MapToDto);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorVendedorAsync(string vendedorId)
    {
        var vendas = await _unitOfWork.Vendas.GetVendasPorVendedorAsync(vendedorId);
        return vendas.Select(MapToDto);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorStatusAsync(string status)
    {
        if (!Enum.TryParse<StatusVenda>(status, true, out var statusEnum))
            throw new ArgumentException($"Status inválido: {status}");

        var vendas = await _unitOfWork.Vendas.GetVendasPorStatusAsync(statusEnum);
        return vendas.Select(MapToDto);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var vendas = await _unitOfWork.Vendas.GetVendasPorPeriodoAsync(dataInicio, dataFim);
        return vendas.Select(MapToDto);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasVencidasAsync()
    {
        var vendas = await _unitOfWork.Vendas.GetVendasVencidasAsync();
        return vendas.Select(MapToDto);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasHojeAsync()
    {
        var vendas = await _unitOfWork.Vendas.GetVendasHojeAsync();
        return vendas.Select(MapToDto);
    }

    public async Task<VendaDto> CreateVendaAsync(CreateVendaDto dto, string? vendedorId = null)
    {
        // Validações
        if (string.IsNullOrEmpty(dto.ClienteId))
            throw new ArgumentException("Cliente é obrigatório");

        if (!dto.Items.Any())
            throw new ArgumentException("Venda deve ter pelo menos um item");

        // Verificar se cliente existe
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new ArgumentException("Cliente não encontrado");

        // Validar forma de pagamento
        if (!Enum.TryParse<FormaPagamento>(dto.FormaPagamento, true, out var formaPagamento))
            throw new ArgumentException($"Forma de pagamento inválida: {dto.FormaPagamento}");

        // Validar e obter dados dos produtos
        var vendaItems = new List<VendaItem>();
        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantidade <= 0)
                throw new ArgumentException($"Quantidade deve ser maior que zero para o produto {itemDto.ProdutoNome}");

            if (itemDto.PrecoUnitario <= 0)
                throw new ArgumentException($"Preço deve ser maior que zero para o produto {itemDto.ProdutoNome}");

            var produto = await _unitOfWork.Produtos.GetByIdAsync(itemDto.ProdutoId);
            if (produto == null)
                throw new ArgumentException($"Produto não encontrado: {itemDto.ProdutoId}");

            // Verificar estoque
            if (produto.Quantidade < itemDto.Quantidade)
                throw new ArgumentException($"Estoque insuficiente para o produto {produto.Nome}. Disponível: {produto.Quantidade}");

            var vendaItem = new VendaItem(
                itemDto.ProdutoId,
                produto.Nome,
                produto.Sku,
                itemDto.Quantidade,
                itemDto.PrecoUnitario
            );

            vendaItems.Add(vendaItem);
        }

        // Obter dados do vendedor se fornecido
        string? vendedorNome = null;
        if (!string.IsNullOrEmpty(vendedorId))
        {
            var vendedor = await _unitOfWork.Usuarios.GetByIdAsync(vendedorId);
            vendedorNome = vendedor?.Nome;
        }

        // Criar venda
        var venda = new Venda
        {
            Numero = await _unitOfWork.Vendas.GetProximoNumeroVendaAsync(),
            ClienteId = dto.ClienteId,
            ClienteNome = cliente.Nome,
            ClienteEmail = cliente.Email?.ToString() ?? string.Empty,
            Items = vendaItems,
            Desconto = dto.Desconto,
            FormaPagamento = formaPagamento,
            Status = StatusVenda.Pendente,
            Observacoes = dto.Observacoes,
            DataVenda = DateTime.UtcNow,
            DataVencimento = !string.IsNullOrEmpty(dto.DataVencimento) ? DateTime.Parse(dto.DataVencimento) : null,
            VendedorId = vendedorId,
            VendedorNome = vendedorNome
        };

        // Calcular valores
        venda.RecalcularValores();

        // Salvar venda
        var vendaCriada = await _unitOfWork.Vendas.CreateAsync(venda);

        // Atualizar estoque dos produtos
        foreach (var item in vendaItems)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
            if (produto != null)
            {
                produto.AtualizarEstoque(produto.Quantidade - item.Quantidade);
                await _unitOfWork.Produtos.UpdateAsync(produto);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(vendaCriada);
    }

    public async Task<VendaDto> UpdateVendaAsync(string id, UpdateVendaDto dto)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        if (venda == null)
            throw new ArgumentException("Venda não encontrada");

        if (!venda.PodeSerAlterada())
            throw new InvalidOperationException("Venda não pode ser alterada");

        // Validar forma de pagamento
        if (!Enum.TryParse<FormaPagamento>(dto.FormaPagamento, true, out var formaPagamento))
            throw new ArgumentException($"Forma de pagamento inválida: {dto.FormaPagamento}");

        // Validar status
        if (!Enum.TryParse<StatusVenda>(dto.Status, true, out var status))
            throw new ArgumentException($"Status inválido: {dto.Status}");

        // Verificar se número já existe (se mudou)
        if (dto.Numero != venda.Numero)
        {
            if (await _unitOfWork.Vendas.NumeroVendaJaExisteAsync(dto.Numero, id))
                throw new ArgumentException($"Número de venda já existe: {dto.Numero}");
        }

        // Verificar se cliente existe (se mudou)
        if (dto.ClienteId != venda.ClienteId)
        {
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
                throw new ArgumentException("Cliente não encontrado");

            venda.ClienteId = dto.ClienteId;
            venda.ClienteNome = cliente.Nome;
            venda.ClienteEmail = cliente.Email?.ToString() ?? string.Empty;
        }

        // Restaurar estoque dos itens antigos
        foreach (var itemAntigo in venda.Items)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(itemAntigo.ProdutoId);
            if (produto != null)
            {
                produto.AtualizarEstoque(produto.Quantidade + itemAntigo.Quantidade);
                await _unitOfWork.Produtos.UpdateAsync(produto);
            }
        }

        // Validar e atualizar itens
        var novosItems = new List<VendaItem>();
        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantidade <= 0)
                throw new ArgumentException($"Quantidade deve ser maior que zero");

            if (itemDto.PrecoUnitario <= 0)
                throw new ArgumentException($"Preço deve ser maior que zero");

            var produto = await _unitOfWork.Produtos.GetByIdAsync(itemDto.ProdutoId);
            if (produto == null)
                throw new ArgumentException($"Produto não encontrado: {itemDto.ProdutoId}");

            // Verificar estoque
            if (produto.Quantidade < itemDto.Quantidade)
                throw new ArgumentException($"Estoque insuficiente para o produto {produto.Nome}. Disponível: {produto.Quantidade}");

            var vendaItem = new VendaItem
            {
                Id = itemDto.Id,
                ProdutoId = itemDto.ProdutoId,
                ProdutoNome = produto.Nome,
                ProdutoSku = produto.Sku,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = itemDto.PrecoUnitario
            };
            vendaItem.CalcularSubtotal();

            novosItems.Add(vendaItem);
        }

        // Atualizar venda
        venda.Numero = dto.Numero;
        venda.Items = novosItems;
        venda.Desconto = dto.Desconto;
        venda.FormaPagamento = formaPagamento;
        venda.Status = status;
        venda.Observacoes = dto.Observacoes;
        venda.DataVencimento = !string.IsNullOrEmpty(dto.DataVencimento) ? DateTime.Parse(dto.DataVencimento) : null;
        venda.VendedorId = dto.VendedorId;
        venda.VendedorNome = dto.VendedorNome;

        venda.RecalcularValores();

        // Atualizar estoque com novos itens
        foreach (var item in novosItems)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
            if (produto != null)
            {
                produto.AtualizarEstoque(produto.Quantidade - item.Quantidade);
                await _unitOfWork.Produtos.UpdateAsync(produto);
            }
        }

        var vendaAtualizada = await _unitOfWork.Vendas.UpdateAsync(venda);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(vendaAtualizada);
    }

    public async Task<bool> DeleteVendaAsync(string id)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        if (venda == null)
            return false;

        if (!venda.PodeSerAlterada())
            throw new InvalidOperationException("Venda não pode ser excluída");

        // Restaurar estoque
        foreach (var item in venda.Items)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
            if (produto != null)
            {
                produto.AtualizarEstoque(produto.Quantidade + item.Quantidade);
                await _unitOfWork.Produtos.UpdateAsync(produto);
            }
        }

        var resultado = await _unitOfWork.Vendas.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return resultado;
    }

    public async Task<VendaDto> ConfirmarVendaAsync(string id)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        if (venda == null)
            throw new ArgumentException("Venda não encontrada");

        venda.Confirmar();

        var vendaAtualizada = await _unitOfWork.Vendas.UpdateAsync(venda);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(vendaAtualizada);
    }

    public async Task<VendaDto> FinalizarVendaAsync(string id)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        if (venda == null)
            throw new ArgumentException("Venda não encontrada");

        venda.Finalizar();

        // Registrar última compra do cliente
        await _unitOfWork.Clientes.GetByIdAsync(venda.ClienteId);
        var cliente = await _unitOfWork.Clientes.GetByIdAsync(venda.ClienteId);
        if (cliente != null)
        {
            cliente.UltimaCompra = DateTime.UtcNow;
            await _unitOfWork.Clientes.UpdateAsync(cliente);
        }

        var vendaAtualizada = await _unitOfWork.Vendas.UpdateAsync(venda);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(vendaAtualizada);
    }

    public async Task<VendaDto> CancelarVendaAsync(string id)
    {
        var venda = await _unitOfWork.Vendas.GetByIdAsync(id);
        if (venda == null)
            throw new ArgumentException("Venda não encontrada");

        venda.Cancelar();

        // Restaurar estoque se a venda ainda não foi finalizada
        if (venda.Status != StatusVenda.Finalizada)
        {
            foreach (var item in venda.Items)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(item.ProdutoId);
                if (produto != null)
                {
                    produto.AtualizarEstoque(produto.Quantidade + item.Quantidade);
                    await _unitOfWork.Produtos.UpdateAsync(produto);
                }
            }
        }

        var vendaAtualizada = await _unitOfWork.Vendas.UpdateAsync(venda);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(vendaAtualizada);
    }

    public async Task<VendasStatsDto> GetVendasStatsAsync()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var totalVendas = (await _unitOfWork.Vendas.GetAllAsync()).Count();
        var vendasHoje = (await _unitOfWork.Vendas.GetVendasHojeAsync()).Count();
        var faturamentoMes = await _unitOfWork.Vendas.GetFaturamentoPorPeriodoAsync(inicioMes, fimMes);
        var vendasPendentes = (await _unitOfWork.Vendas.GetVendasPorStatusAsync(StatusVenda.Pendente)).Count();

        var todasVendas = await _unitOfWork.Vendas.GetAllAsync();
        var vendasFinalizadas = todasVendas.Where(v => v.Status == StatusVenda.Finalizada);
        var ticketMedio = vendasFinalizadas.Any() ? vendasFinalizadas.Average(v => v.Total) : 0;

        var topClientes = (await _unitOfWork.Vendas.GetTopClientesAsync(5))
            .Select(c => new TopClienteDto
            {
                ClienteNome = (string)(c.GetType().GetProperty("ClienteNome")?.GetValue(c) ?? ""),
                TotalCompras = (int)(c.GetType().GetProperty("TotalCompras")?.GetValue(c) ?? 0),
                ValorTotal = (decimal)(c.GetType().GetProperty("ValorTotal")?.GetValue(c) ?? 0m)
            });

        var vendasPorMes = (await _unitOfWork.Vendas.GetVendasPorMesAsync(6))
            .Select(v => new VendasPorMesDto
            {
                Mes = (string)(v.GetType().GetProperty("Mes")?.GetValue(v) ?? ""),
                Vendas = (int)(v.GetType().GetProperty("Vendas")?.GetValue(v) ?? 0),
                Faturamento = (decimal)(v.GetType().GetProperty("Faturamento")?.GetValue(v) ?? 0m)
            });

        return new VendasStatsDto
        {
            TotalVendas = totalVendas,
            VendasHoje = vendasHoje,
            FaturamentoMes = faturamentoMes,
            TicketMedio = ticketMedio,
            VendasPendentes = vendasPendentes,
            TopClientes = topClientes,
            VendasPorMes = vendasPorMes
        };
    }

    public async Task<string> GetProximoNumeroVendaAsync()
    {
        return await _unitOfWork.Vendas.GetProximoNumeroVendaAsync();
    }

    private static VendaDto MapToDto(Venda venda)
    {
        return new VendaDto
        {
            Id = venda.Id,
            Numero = venda.Numero,
            ClienteId = venda.ClienteId,
            ClienteNome = venda.ClienteNome,
            ClienteEmail = venda.ClienteEmail,
            Items = venda.Items.Select(item => new VendaItemDto
            {
                Id = item.Id,
                ProdutoId = item.ProdutoId,
                ProdutoNome = item.ProdutoNome,
                ProdutoSku = item.ProdutoSku,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Subtotal = item.Subtotal
            }),
            Subtotal = venda.Subtotal,
            Desconto = venda.Desconto,
            Total = venda.Total,
            FormaPagamento = GetFormaPagamentoString(venda.FormaPagamento),
            Status = venda.Status.ToString(),
            Observacoes = venda.Observacoes,
            DataVenda = venda.DataVenda.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            DataVencimento = venda.DataVencimento?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            VendedorId = venda.VendedorId,
            VendedorNome = venda.VendedorNome,
            CreatedAt = venda.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            UpdatedAt = venda.DataAtualizacao.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
    }

    private static string GetFormaPagamentoString(FormaPagamento formaPagamento)
    {
        return formaPagamento switch
        {
            FormaPagamento.Dinheiro => "Dinheiro",
            FormaPagamento.CartaoCredito => "Cartão de Crédito",
            FormaPagamento.CartaoDebito => "Cartão de Débito",
            FormaPagamento.PIX => "PIX",
            FormaPagamento.Boleto => "Boleto",
            _ => formaPagamento.ToString()
        };
    }
}