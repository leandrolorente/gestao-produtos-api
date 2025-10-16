using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Application.Services;

/// <summary>
/// Serviço para gestão de contas a pagar
/// </summary>
public class ContaPagarService : IContaPagarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ContaPagarService> _logger;

    public ContaPagarService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ContaPagarService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<ContaPagarDto>> GetAllContasPagarAsync()
    {
        try
        {
            // Tentar buscar do cache
            var cacheKey = "gp:contas-pagar:all";
            var cached = await _cacheService.GetAsync<List<ContaPagarDto>>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogInformation("✅ [CACHE HIT] Contas a pagar retornadas do cache");
                Console.WriteLine("✅ [REDIS HIT] Contas a pagar encontradas no cache");
                return cached;
            }

            // Buscar do banco
            var contas = await _unitOfWork.ContasPagar.GetAllAsync();
            var resultado = contas.Select(MapToDto).ToList();

            // Salvar no cache (2 minutos)
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(2));
            
            _logger.LogInformation("🗄️ [DATABASE] Contas a pagar buscadas no MongoDB e salvas no cache");
            Console.WriteLine("💾 [REDIS SET] Contas a pagar salvas no cache");

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as contas a pagar");
            throw;
        }
    }

    public async Task<ContaPagarDto?> GetContaPagarByIdAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(id);
            return conta != null ? MapToDto(conta) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conta a pagar por ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarByStatusAsync(StatusContaPagar status)
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetByStatusAsync(status);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarVencidasAsync()
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetVencidasAsync();
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar vencidas");
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarByFornecedorAsync(string fornecedorId)
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetByFornecedorAsync(fornecedorId);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por fornecedor: {FornecedorId}", fornecedorId);
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetByPeriodoAsync(inicio, fim);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarVencendoEmAsync(int dias)
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetVencendoEmAsync(dias);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar vencendo em {Dias} dias", dias);
            throw;
        }
    }

    public async Task<IEnumerable<ContaPagarDto>> GetContasPagarByCategoriaAsync(CategoriaConta categoria)
    {
        try
        {
            var contas = await _unitOfWork.ContasPagar.GetByCategoriaAsync(categoria);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a pagar por categoria: {Categoria}", categoria);
            throw;
        }
    }

    public async Task<ContaPagarDto> CreateContaPagarAsync(CreateContaPagarDto dto)
    {
        try
        {
            // Buscar fornecedor se informado
            string? fornecedorNome = null;
            if (!string.IsNullOrEmpty(dto.FornecedorId))
            {
                var fornecedor = await _unitOfWork.Fornecedores.GetByIdAsync(dto.FornecedorId);
                fornecedorNome = fornecedor?.RazaoSocial;
            }

            var conta = new ContaPagar
            {
                Descricao = dto.Descricao,
                FornecedorId = dto.FornecedorId,
                FornecedorNome = fornecedorNome,
                CompraId = dto.CompraId,
                NotaFiscal = dto.NotaFiscal,
                ValorOriginal = dto.ValorOriginal,
                Desconto = dto.Desconto,
                DataEmissao = dto.DataEmissao,
                DataVencimento = dto.DataVencimento,
                Status = StatusContaPagar.Pendente,
                Categoria = dto.Categoria,
                EhRecorrente = dto.EhRecorrente,
                TipoRecorrencia = dto.TipoRecorrencia,
                DiasRecorrencia = dto.DiasRecorrencia,
                Observacoes = dto.Observacoes,
                CentroCusto = dto.CentroCusto
            };

            var contaCriada = await _unitOfWork.ContasPagar.CreateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasPagar();

            _logger.LogInformation("Conta a pagar criada com sucesso: {Numero} - {Descricao}", 
                contaCriada.Numero, contaCriada.Descricao);

            return MapToDto(contaCriada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta a pagar: {Descricao}", dto.Descricao);
            throw;
        }
    }

    public async Task<ContaPagarDto> UpdateContaPagarAsync(string id, UpdateContaPagarDto dto)
    {
        try
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(id);
            if (conta == null)
            {
                throw new InvalidOperationException("Conta a pagar não encontrada");
            }

            if (conta.Status == StatusContaPagar.Paga)
            {
                throw new InvalidOperationException("Não é possível alterar conta já paga");
            }

            // Atualizar dados
            conta.Descricao = dto.Descricao;
            conta.NotaFiscal = dto.NotaFiscal;
            conta.ValorOriginal = dto.ValorOriginal;
            conta.Desconto = dto.Desconto;
            conta.DataEmissao = dto.DataEmissao;
            conta.DataVencimento = dto.DataVencimento;
            conta.Categoria = dto.Categoria;
            conta.EhRecorrente = dto.EhRecorrente;
            conta.TipoRecorrencia = dto.TipoRecorrencia;
            conta.DiasRecorrencia = dto.DiasRecorrencia;
            conta.Observacoes = dto.Observacoes;
            conta.CentroCusto = dto.CentroCusto;

            var contaAtualizada = await _unitOfWork.ContasPagar.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasPagar();

            _logger.LogInformation("Conta a pagar atualizada com sucesso: {Id} - {Descricao}", id, conta.Descricao);

            return MapToDto(contaAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta a pagar: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteContaPagarAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(id);
            if (conta == null)
            {
                return false;
            }

            if (conta.Status == StatusContaPagar.Paga)
            {
                throw new InvalidOperationException("Não é possível excluir conta já paga");
            }

            var resultado = await _unitOfWork.ContasPagar.DeleteAsync(id);

            if (resultado)
            {
                // Invalidar cache
                await InvalidarCacheContasPagar();
                _logger.LogInformation("Conta a pagar excluída com sucesso: {Id}", id);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta a pagar: {Id}", id);
            throw;
        }
    }

    public async Task<ContaPagarDto> PagarContaAsync(string id, PagarContaDto dto)
    {
        try
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(id);
            if (conta == null)
            {
                throw new InvalidOperationException("Conta a pagar não encontrada");
            }

            conta.Pagar(dto.Valor, dto.FormaPagamento, dto.DataPagamento);
            
            if (!string.IsNullOrEmpty(dto.Observacoes))
            {
                conta.Observacoes = $"{conta.Observacoes}\n[Pagamento] {dto.Observacoes}".Trim();
            }

            var contaAtualizada = await _unitOfWork.ContasPagar.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasPagar();

            _logger.LogInformation("Pagamento realizado com sucesso: {Id} - Valor: {Valor}", id, dto.Valor);

            return MapToDto(contaAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar pagamento da conta: {Id}", id);
            throw;
        }
    }

    public async Task<bool> CancelarContaAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasPagar.GetByIdAsync(id);
            if (conta == null)
            {
                return false;
            }

            conta.Cancelar();
            await _unitOfWork.ContasPagar.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasPagar();

            _logger.LogInformation("Conta a pagar cancelada com sucesso: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar conta a pagar: {Id}", id);
            throw;
        }
    }

    public async Task<decimal> GetTotalPagarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            return await _unitOfWork.ContasPagar.GetTotalPagarPorPeriodoAsync(inicio, fim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total a pagar por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<decimal> GetTotalPagoPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            return await _unitOfWork.ContasPagar.GetTotalPagoPorPeriodoAsync(inicio, fim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total pago por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<int> GetQuantidadeContasVencidasAsync()
    {
        try
        {
            return await _unitOfWork.ContasPagar.GetQuantidadeVencidasAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar contas vencidas");
            throw;
        }
    }

    public async Task AtualizarStatusContasAsync()
    {
        try
        {
            var contasPendentes = await _unitOfWork.ContasPagar.GetByStatusAsync(StatusContaPagar.Pendente);
            var contasParciais = await _unitOfWork.ContasPagar.GetByStatusAsync(StatusContaPagar.PagamentoParcial);
            
            var contasParaAtualizar = contasPendentes.Concat(contasParciais);

            foreach (var conta in contasParaAtualizar)
            {
                conta.AtualizarStatus();
                await _unitOfWork.ContasPagar.UpdateAsync(conta);
            }

            // Invalidar cache
            await InvalidarCacheContasPagar();

            _logger.LogInformation("Status das contas a pagar atualizados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status das contas a pagar");
            throw;
        }
    }

    public async Task ProcessarContasRecorrentesAsync()
    {
        try
        {
            var contasRecorrentes = await _unitOfWork.ContasPagar.GetRecorrentesParaGerarAsync();

            foreach (var conta in contasRecorrentes)
            {
                // Implementar lógica para gerar próximas parcelas
                // Este seria executado por um background job
                conta.GerarProximaParcela();
            }

            _logger.LogInformation("Contas recorrentes processadas: {Quantidade}", contasRecorrentes.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar contas recorrentes");
            throw;
        }
    }

    private async Task InvalidarCacheContasPagar()
    {
        await _cacheService.RemoveAsync("gp:contas-pagar:all");
        await _cacheService.RemovePatternAsync("gp:contas-pagar:*");
    }

    private static ContaPagarDto MapToDto(ContaPagar conta)
    {
        var diasVencimento = (conta.DataVencimento.Date - DateTime.UtcNow.Date).Days;
        
        return new ContaPagarDto
        {
            Id = conta.Id,
            Numero = conta.Numero,
            Descricao = conta.Descricao,
            FornecedorId = conta.FornecedorId,
            FornecedorNome = conta.FornecedorNome,
            CompraId = conta.CompraId,
            NotaFiscal = conta.NotaFiscal,
            ValorOriginal = conta.ValorOriginal,
            Desconto = conta.Desconto,
            Juros = conta.Juros,
            Multa = conta.Multa,
            ValorPago = conta.ValorPago,
            ValorRestante = conta.ValorRestante,
            DataEmissao = conta.DataEmissao,
            DataVencimento = conta.DataVencimento,
            DataPagamento = conta.DataPagamento,
            Status = ((int)conta.Status).ToString(),
            Categoria = ((int)conta.Categoria).ToString(),
            FormaPagamento = conta.FormaPagamento?.ToString(),
            EhRecorrente = conta.EhRecorrente,
            TipoRecorrencia = conta.TipoRecorrencia?.ToString(),
            DiasRecorrencia = conta.DiasRecorrencia,
            Observacoes = conta.Observacoes,
            CentroCusto = conta.CentroCusto,
            EstaVencida = conta.EstaVencida(),
            DiasVencimento = diasVencimento,
            DataCriacao = conta.DataCriacao,
            DataAtualizacao = conta.DataAtualizacao,
            Ativo = conta.Ativo
        };
    }
}
