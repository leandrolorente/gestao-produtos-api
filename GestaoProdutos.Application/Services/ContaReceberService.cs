using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Application.Services;

/// <summary>
/// Serviço para gestão de contas a receber
/// </summary>
public class ContaReceberService : IContaReceberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ContaReceberService> _logger;

    public ContaReceberService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ContaReceberService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<ContaReceberDto>> GetAllContasReceberAsync()
    {
        try
        {
            // Tentar buscar do cache
            var cacheKey = "gp:contas-receber:all";
            var cached = await _cacheService.GetAsync<List<ContaReceberDto>>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogInformation("✅ [CACHE HIT] Contas a receber retornadas do cache");
                Console.WriteLine("✅ [REDIS HIT] Contas a receber encontradas no cache");
                return cached;
            }

            // Buscar do banco
            var contas = await _unitOfWork.ContasReceber.GetAllAsync();
            var resultado = contas.Select(MapToDto).ToList();

            // Salvar no cache (2 minutos)
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(2));
            
            _logger.LogInformation("🗄️ [DATABASE] Contas a receber buscadas no MongoDB e salvas no cache");
            Console.WriteLine("💾 [REDIS SET] Contas a receber salvas no cache");

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as contas a receber");
            throw;
        }
    }

    public async Task<ContaReceberDto?> GetContaReceberByIdAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(id);
            return conta != null ? MapToDto(conta) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conta a receber por ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberByStatusAsync(StatusContaReceber status)
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetByStatusAsync(status);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberVencidasAsync()
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetVencidasAsync();
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber vencidas");
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberByClienteAsync(string clienteId)
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetByClienteAsync(clienteId);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por cliente: {ClienteId}", clienteId);
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetByPeriodoAsync(inicio, fim);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberVencendoEmAsync(int dias)
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetVencendoEmAsync(dias);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber vencendo em {Dias} dias", dias);
            throw;
        }
    }

    public async Task<ContaReceberDto> CreateContaReceberAsync(CreateContaReceberDto dto)
    {
        try
        {
            // Buscar cliente se informado
            string? clienteNome = null;
            if (!string.IsNullOrEmpty(dto.ClienteId))
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId);
                clienteNome = cliente?.Nome;
            }

            // Buscar vendedor se informado
            string? vendedorNome = null;
            if (!string.IsNullOrEmpty(dto.VendedorId))
            {
                var vendedor = await _unitOfWork.Usuarios.GetByIdAsync(dto.VendedorId);
                vendedorNome = vendedor?.Nome;
            }

            var conta = new ContaReceber
            {
                Descricao = dto.Descricao,
                ClienteId = dto.ClienteId,
                ClienteNome = clienteNome,
                VendaId = dto.VendaId,
                NotaFiscal = dto.NotaFiscal,
                ValorOriginal = dto.ValorOriginal,
                Desconto = dto.Desconto,
                DataEmissao = dto.DataEmissao,
                DataVencimento = dto.DataVencimento,
                Status = StatusContaReceber.Pendente,
                EhRecorrente = dto.EhRecorrente,
                TipoRecorrencia = dto.TipoRecorrencia,
                Observacoes = dto.Observacoes,
                VendedorId = dto.VendedorId,
                VendedorNome = vendedorNome
            };

            var contaCriada = await _unitOfWork.ContasReceber.CreateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasReceber();

            _logger.LogInformation("Conta a receber criada com sucesso: {Numero} - {Descricao}", 
                contaCriada.Numero, contaCriada.Descricao);

            return MapToDto(contaCriada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta a receber: {Descricao}", dto.Descricao);
            throw;
        }
    }

    public async Task<ContaReceberDto> UpdateContaReceberAsync(string id, UpdateContaReceberDto dto)
    {
        try
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(id);
            if (conta == null)
            {
                throw new InvalidOperationException("Conta a receber não encontrada");
            }

            if (conta.Status == StatusContaReceber.Recebida)
            {
                throw new InvalidOperationException("Não é possível alterar conta já recebida");
            }

            // Buscar vendedor se informado
            string? vendedorNome = null;
            if (!string.IsNullOrEmpty(dto.VendedorId))
            {
                var vendedor = await _unitOfWork.Usuarios.GetByIdAsync(dto.VendedorId);
                vendedorNome = vendedor?.Nome;
            }

            // Atualizar dados
            conta.Descricao = dto.Descricao;
            conta.NotaFiscal = dto.NotaFiscal;
            conta.ValorOriginal = dto.ValorOriginal;
            conta.Desconto = dto.Desconto;
            conta.DataEmissao = dto.DataEmissao;
            conta.DataVencimento = dto.DataVencimento;
            conta.EhRecorrente = dto.EhRecorrente;
            conta.TipoRecorrencia = dto.TipoRecorrencia;
            conta.Observacoes = dto.Observacoes;
            conta.VendedorId = dto.VendedorId;
            conta.VendedorNome = vendedorNome;

            var contaAtualizada = await _unitOfWork.ContasReceber.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasReceber();

            _logger.LogInformation("Conta a receber atualizada com sucesso: {Id} - {Descricao}", id, conta.Descricao);

            return MapToDto(contaAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta a receber: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteContaReceberAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(id);
            if (conta == null)
            {
                return false;
            }

            if (conta.Status == StatusContaReceber.Recebida)
            {
                throw new InvalidOperationException("Não é possível excluir conta já recebida");
            }

            var resultado = await _unitOfWork.ContasReceber.DeleteAsync(id);

            if (resultado)
            {
                // Invalidar cache
                await InvalidarCacheContasReceber();
                _logger.LogInformation("Conta a receber excluída com sucesso: {Id}", id);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta a receber: {Id}", id);
            throw;
        }
    }

    public async Task<ContaReceberDto> ReceberContaAsync(string id, ReceberContaDto dto)
    {
        try
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(id);
            if (conta == null)
            {
                throw new InvalidOperationException("Conta a receber não encontrada");
            }

            conta.Receber(dto.Valor, dto.FormaPagamento, dto.DataRecebimento);
            
            if (!string.IsNullOrEmpty(dto.Observacoes))
            {
                conta.Observacoes = $"{conta.Observacoes}\n[Recebimento] {dto.Observacoes}".Trim();
            }

            var contaAtualizada = await _unitOfWork.ContasReceber.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasReceber();

            _logger.LogInformation("Recebimento realizado com sucesso: {Id} - Valor: {Valor}", id, dto.Valor);

            return MapToDto(contaAtualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar recebimento da conta: {Id}", id);
            throw;
        }
    }

    public async Task<bool> CancelarContaAsync(string id)
    {
        try
        {
            var conta = await _unitOfWork.ContasReceber.GetByIdAsync(id);
            if (conta == null)
            {
                return false;
            }

            conta.Cancelar();
            await _unitOfWork.ContasReceber.UpdateAsync(conta);

            // Invalidar cache
            await InvalidarCacheContasReceber();

            _logger.LogInformation("Conta a receber cancelada com sucesso: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar conta a receber: {Id}", id);
            throw;
        }
    }

    public async Task<decimal> GetTotalReceberPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            return await _unitOfWork.ContasReceber.GetTotalReceberPorPeriodoAsync(inicio, fim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total a receber por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<decimal> GetTotalRecebidoPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        try
        {
            return await _unitOfWork.ContasReceber.GetTotalRecebidoPorPeriodoAsync(inicio, fim);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular total recebido por período: {Inicio} - {Fim}", inicio, fim);
            throw;
        }
    }

    public async Task<int> GetQuantidadeContasVencidasAsync()
    {
        try
        {
            return await _unitOfWork.ContasReceber.GetQuantidadeVencidasAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar contas vencidas");
            throw;
        }
    }

    public async Task<IEnumerable<ContaReceberDto>> GetContasReceberByVendedorAsync(string vendedorId)
    {
        try
        {
            var contas = await _unitOfWork.ContasReceber.GetByVendedorAsync(vendedorId);
            return contas.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar contas a receber por vendedor: {VendedorId}", vendedorId);
            throw;
        }
    }

    public async Task AtualizarStatusContasAsync()
    {
        try
        {
            var contasPendentes = await _unitOfWork.ContasReceber.GetByStatusAsync(StatusContaReceber.Pendente);
            var contasParciais = await _unitOfWork.ContasReceber.GetByStatusAsync(StatusContaReceber.RecebimentoParcial);
            
            var contasParaAtualizar = contasPendentes.Concat(contasParciais);

            foreach (var conta in contasParaAtualizar)
            {
                conta.AtualizarStatus();
                await _unitOfWork.ContasReceber.UpdateAsync(conta);
            }

            // Invalidar cache
            await InvalidarCacheContasReceber();

            _logger.LogInformation("Status das contas a receber atualizados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status das contas a receber");
            throw;
        }
    }

    public async Task ProcessarContasRecorrentesAsync()
    {
        try
        {
            var contasRecorrentes = await _unitOfWork.ContasReceber.GetRecorrentesParaGerarAsync();

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

    private async Task InvalidarCacheContasReceber()
    {
        await _cacheService.RemoveAsync("gp:contas-receber:all");
        await _cacheService.RemovePatternAsync("gp:contas-receber:*");
    }

    private static ContaReceberDto MapToDto(ContaReceber conta)
    {
        var diasVencimento = (conta.DataVencimento.Date - DateTime.UtcNow.Date).Days;
        
        return new ContaReceberDto
        {
            Id = conta.Id,
            Numero = conta.Numero,
            Descricao = conta.Descricao,
            ClienteId = conta.ClienteId,
            ClienteNome = conta.ClienteNome,
            VendaId = conta.VendaId,
            NotaFiscal = conta.NotaFiscal,
            ValorOriginal = conta.ValorOriginal,
            Desconto = conta.Desconto,
            Juros = conta.Juros,
            Multa = conta.Multa,
            ValorRecebido = conta.ValorRecebido,
            ValorRestante = conta.ValorRestante,
            DataEmissao = conta.DataEmissao,
            DataVencimento = conta.DataVencimento,
            DataRecebimento = conta.DataRecebimento,
            Status = ((int)conta.Status).ToString(),
            FormaPagamento = conta.FormaPagamento?.ToString(),
            EhRecorrente = conta.EhRecorrente,
            TipoRecorrencia = conta.TipoRecorrencia?.ToString(),
            Observacoes = conta.Observacoes,
            VendedorId = conta.VendedorId,
            VendedorNome = conta.VendedorNome,
            EstaVencida = conta.EstaVencida(),
            DiasVencimento = diasVencimento,
            DataCriacao = conta.DataCriacao,
            DataAtualizacao = conta.DataAtualizacao,
            Ativo = conta.Ativo
        };
    }
}
