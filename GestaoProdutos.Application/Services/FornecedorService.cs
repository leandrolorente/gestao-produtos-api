using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using GestaoProdutos.Domain.Entities;
using GestaoProdutos.Domain.Enums;
using GestaoProdutos.Domain.Interfaces;
using GestaoProdutos.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Application.Services;

/// <summary>
/// Serviço para operações com fornecedores
/// </summary>
public class FornecedorService : IFornecedorService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<FornecedorService> _logger;

    public FornecedorService(
        IFornecedorRepository fornecedorRepository,
        ICacheService cacheService,
        ILogger<FornecedorService> logger)
    {
        _fornecedorRepository = fornecedorRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<FornecedorDto>> GetAllFornecedoresAsync()
    {
        try
        {
            // Tentar buscar do cache
            var cacheKey = "gp:fornecedores:all";
            var cached = await _cacheService.GetAsync<List<FornecedorDto>>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogInformation("✅ [CACHE HIT] Fornecedores retornados do cache");
                Console.WriteLine("✅ [REDIS HIT] Fornecedores encontrados no cache");
                return cached;
            }

            // Buscar do banco
            var fornecedores = await _fornecedorRepository.GetAllAsync();
            var resultado = fornecedores.Select(MapToDto).ToList();

            // Salvar no cache (5 minutos)
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(5));
            
            _logger.LogInformation("🗄️ [DATABASE] Fornecedores buscados no MongoDB e salvos no cache");
            Console.WriteLine("💾 [REDIS SET] Fornecedores salvos no cache");

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os fornecedores");
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorListDto>> GetAllFornecedoresListAsync()
    {
        try
        {
            var cacheKey = "gp:fornecedores:list";
            var cached = await _cacheService.GetAsync<List<FornecedorListDto>>(cacheKey);
            
            if (cached != null)
            {
                return cached;
            }

            var fornecedores = await _fornecedorRepository.GetAllAsync();
            var resultado = fornecedores.Select(MapToListDto).ToList();

            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(5));
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lista de fornecedores");
            throw;
        }
    }

    public async Task<FornecedorDto?> GetFornecedorByIdAsync(string id)
    {
        try
        {
            var cacheKey = $"gp:fornecedor:{id}";
            var cached = await _cacheService.GetAsync<FornecedorDto>(cacheKey);
            
            if (cached != null)
            {
                return cached;
            }

            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null) return null;

            var resultado = MapToDto(fornecedor);
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(10));
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedor por ID: {Id}", id);
            throw;
        }
    }

    public async Task<FornecedorDto?> GetFornecedorByCnpjCpfAsync(string cnpjCpf)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetFornecedorPorCnpjCpfAsync(cnpjCpf);
            return fornecedor != null ? MapToDto(fornecedor) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedor por CNPJ/CPF: {CnpjCpf}", cnpjCpf);
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> GetFornecedoresAtivosPorTipoAsync(TipoFornecedor tipo)
    {
        try
        {
            var fornecedores = await _fornecedorRepository.GetFornecedoresAtivosPorTipoAsync(tipo);
            return fornecedores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores ativos por tipo: {Tipo}", tipo);
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> GetFornecedoresComCompraRecenteAsync(int dias = 90)
    {
        try
        {
            var fornecedores = await _fornecedorRepository.GetFornecedoresComCompraRecenteAsync(dias);
            return fornecedores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores com compra recente");
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> GetFornecedoresPorStatusAsync(StatusFornecedor status)
    {
        try
        {
            var fornecedores = await _fornecedorRepository.GetFornecedoresPorStatusAsync(status);
            return fornecedores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores por status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> GetFornecedoresPorProdutoAsync(string produtoId)
    {
        try
        {
            var fornecedores = await _fornecedorRepository.GetFornecedoresPorProdutoAsync(produtoId);
            return fornecedores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores por produto: {ProdutoId}", produtoId);
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> GetFornecedoresFrequentesAsync()
    {
        try
        {
            var cacheKey = "gp:fornecedores:frequentes";
            var cached = await _cacheService.GetAsync<List<FornecedorDto>>(cacheKey);
            
            if (cached != null)
            {
                return cached;
            }

            var fornecedores = await _fornecedorRepository.GetFornecedoresFrequentesAsync();
            var resultado = fornecedores.Select(MapToDto).ToList();

            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(15));
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores frequentes");
            throw;
        }
    }

    public async Task<IEnumerable<FornecedorDto>> BuscarFornecedoresAsync(string termo)
    {
        try
        {
            var fornecedores = await _fornecedorRepository.BuscarFornecedoresAsync(termo);
            return fornecedores.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar fornecedores com termo: {Termo}", termo);
            throw;
        }
    }

    public async Task<FornecedorDto> CreateFornecedorAsync(CreateFornecedorDto dto)
    {
        try
        {
            // Validar se CNPJ/CPF já existe
            var existeFornecedor = await _fornecedorRepository.CnpjCpfJaExisteAsync(dto.CnpjCpf, null);
            if (existeFornecedor)
            {
                throw new InvalidOperationException($"Já existe um fornecedor com o CNPJ/CPF: {dto.CnpjCpf}");
            }

            // Mapear DTO para entidade
            var fornecedor = new Fornecedor
            {
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                CnpjCpf = new CpfCnpj(dto.CnpjCpf),
                Email = new Email(dto.Email),
                Telefone = dto.Telefone,
                InscricaoEstadual = dto.InscricaoEstadual,
                InscricaoMunicipal = dto.InscricaoMunicipal,
                Tipo = dto.Tipo,
                Status = StatusFornecedor.Ativo,
                Observacoes = dto.Observacoes,
                ContatoPrincipal = dto.ContatoPrincipal,
                Site = dto.Site,
                Banco = dto.Banco,
                Agencia = dto.Agencia,
                Conta = dto.Conta,
                Pix = dto.Pix,
                PrazoPagamentoPadrao = dto.PrazoPagamentoPadrao,
                LimiteCredito = dto.LimiteCredito,
                CondicoesPagamento = dto.CondicoesPagamento
            };

            // Mapear endereço se fornecido
            if (dto.Endereco != null)
            {
                fornecedor.Endereco = new Endereco
                {
                    Logradouro = dto.Endereco.Logradouro,
                    Cidade = dto.Endereco.Localidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep,
                    Complemento = dto.Endereco.Complemento
                };
            }

            var fornecedorCriado = await _fornecedorRepository.CreateAsync(fornecedor);

            // Invalidar cache
            await InvalidarCacheFornecedores();

            _logger.LogInformation("Fornecedor criado com sucesso: {Id} - {RazaoSocial}", fornecedorCriado.Id, fornecedorCriado.RazaoSocial);

            return MapToDto(fornecedorCriado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar fornecedor: {RazaoSocial}", dto.RazaoSocial);
            throw;
        }
    }

    public async Task<FornecedorDto> UpdateFornecedorAsync(string id, UpdateFornecedorDto dto)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null)
            {
                throw new InvalidOperationException("Fornecedor não encontrado");
            }

            // Atualizar dados básicos
            fornecedor.AtualizarDados(dto.RazaoSocial, dto.NomeFantasia, dto.Telefone, dto.ContatoPrincipal);
            
            // Atualizar email
            fornecedor.Email = new Email(dto.Email);
            
            // Atualizar outros campos
            fornecedor.InscricaoEstadual = dto.InscricaoEstadual;
            fornecedor.InscricaoMunicipal = dto.InscricaoMunicipal;
            fornecedor.Tipo = dto.Tipo;
            fornecedor.Status = dto.Status;
            fornecedor.Observacoes = dto.Observacoes;
            fornecedor.Site = dto.Site;

            // Atualizar endereço
            if (dto.Endereco != null)
            {
                fornecedor.Endereco = new Endereco
                {
                    Logradouro = dto.Endereco.Logradouro,
                    Cidade = dto.Endereco.Localidade,
                    Estado = dto.Endereco.Estado,
                    Cep = dto.Endereco.Cep,
                    Complemento = dto.Endereco.Complemento
                };
            }

            // Atualizar condições comerciais
            fornecedor.AtualizarCondicoesComerciais(dto.PrazoPagamentoPadrao, dto.LimiteCredito, dto.CondicoesPagamento);
            
            // Atualizar dados bancários
            fornecedor.AtualizarDadosBancarios(dto.Banco, dto.Agencia, dto.Conta, dto.Pix);

            var fornecedorAtualizado = await _fornecedorRepository.UpdateAsync(fornecedor);

            // Invalidar cache
            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor atualizado com sucesso: {Id} - {RazaoSocial}", id, fornecedor.RazaoSocial);

            return MapToDto(fornecedorAtualizado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteFornecedorAsync(string id)
    {
        try
        {
            var resultado = await _fornecedorRepository.DeleteAsync(id);
            
            if (resultado)
            {
                await InvalidarCacheFornecedores();
                await _cacheService.RemoveAsync($"gp:fornecedor:{id}");
                _logger.LogInformation("Fornecedor excluído com sucesso: {Id}", id);
            }

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> BloquearFornecedorAsync(string id, string? motivo)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null) return false;

            fornecedor.Bloquear(motivo);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor bloqueado: {Id} - Motivo: {Motivo}", id, motivo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao bloquear fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DesbloquearFornecedorAsync(string id)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null) return false;

            fornecedor.Desbloquear();
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor desbloqueado: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desbloquear fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> InativarFornecedorAsync(string id)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null) return false;

            fornecedor.Inativar();
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor inativado: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inativar fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> AtivarFornecedorAsync(string id)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            if (fornecedor == null) return false;

            fornecedor.Status = StatusFornecedor.Ativo;
            fornecedor.DataAtualizacao = DateTime.UtcNow;
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor ativado: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar fornecedor: {Id}", id);
            throw;
        }
    }

    public async Task<bool> AdicionarProdutoAsync(string fornecedorId, string produtoId)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.AdicionarProduto(produtoId);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await _cacheService.RemoveAsync($"gp:fornecedor:{fornecedorId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar produto ao fornecedor: {FornecedorId}, {ProdutoId}", fornecedorId, produtoId);
            throw;
        }
    }

    public async Task<bool> RemoverProdutoAsync(string fornecedorId, string produtoId)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.RemoverProduto(produtoId);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await _cacheService.RemoveAsync($"gp:fornecedor:{fornecedorId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover produto do fornecedor: {FornecedorId}, {ProdutoId}", fornecedorId, produtoId);
            throw;
        }
    }

    public async Task<bool> RegistrarCompraAsync(string fornecedorId, decimal valor)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.RegistrarCompra(valor);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{fornecedorId}");

            _logger.LogInformation("Compra registrada para fornecedor: {FornecedorId} - Valor: {Valor:C}", fornecedorId, valor);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar compra: {FornecedorId}, {Valor}", fornecedorId, valor);
            throw;
        }
    }

    public async Task<bool> AtualizarCondicoesComerciais(string fornecedorId, int prazoPagamento, decimal limiteCredito, string? condicoesPagamento)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.AtualizarCondicoesComerciais(prazoPagamento, limiteCredito, condicoesPagamento);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await _cacheService.RemoveAsync($"gp:fornecedor:{fornecedorId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar condições comerciais: {FornecedorId}", fornecedorId);
            throw;
        }
    }

    public async Task<bool> AtualizarDadosBancarios(string fornecedorId, string? banco, string? agencia, string? conta, string? pix)
    {
        try
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(fornecedorId);
            if (fornecedor == null) return false;

            fornecedor.AtualizarDadosBancarios(banco, agencia, conta, pix);
            await _fornecedorRepository.UpdateAsync(fornecedor);

            await _cacheService.RemoveAsync($"gp:fornecedor:{fornecedorId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar dados bancários: {FornecedorId}", fornecedorId);
            throw;
        }
    }

    // Métodos auxiliares de mapeamento

    private static FornecedorDto MapToDto(Fornecedor fornecedor)
    {
        return new FornecedorDto
        {
            Id = fornecedor.Id,
            RazaoSocial = fornecedor.RazaoSocial,
            NomeFantasia = fornecedor.NomeFantasia,
            CnpjCpf = fornecedor.CnpjCpf.Valor,
            Email = fornecedor.Email.Valor,
            Telefone = fornecedor.Telefone,
            Endereco = fornecedor.Endereco != null ? new EnderecoDto
            {
                Logradouro = fornecedor.Endereco.Logradouro,
                Localidade = fornecedor.Endereco.Cidade,
                Estado = fornecedor.Endereco.Estado,
                Cep = fornecedor.Endereco.Cep,
                Complemento = fornecedor.Endereco.Complemento ?? string.Empty
            } : null,
            InscricaoEstadual = fornecedor.InscricaoEstadual,
            InscricaoMunicipal = fornecedor.InscricaoMunicipal,
            Tipo = fornecedor.Tipo.ToString(),
            Status = fornecedor.Status.ToString(),
            Observacoes = fornecedor.Observacoes,
            ContatoPrincipal = fornecedor.ContatoPrincipal,
            Site = fornecedor.Site,
            Banco = fornecedor.Banco,
            Agencia = fornecedor.Agencia,
            Conta = fornecedor.Conta,
            Pix = fornecedor.Pix,
            PrazoPagamentoPadrao = fornecedor.PrazoPagamentoPadrao,
            LimiteCredito = fornecedor.LimiteCredito,
            CondicoesPagamento = fornecedor.CondicoesPagamento,
            QuantidadeProdutos = fornecedor.ProdutoIds.Count,
            UltimaCompra = fornecedor.UltimaCompra,
            TotalComprado = fornecedor.TotalComprado,
            QuantidadeCompras = fornecedor.QuantidadeCompras,
            TicketMedio = fornecedor.CalcularTicketMedio(),
            EhFrequente = fornecedor.EhFrequente(),
            DataCriacao = fornecedor.DataCriacao,
            DataAtualizacao = fornecedor.DataAtualizacao,
            Ativo = fornecedor.Ativo
        };
    }

    private static FornecedorListDto MapToListDto(Fornecedor fornecedor)
    {
        return new FornecedorListDto
        {
            Id = fornecedor.Id,
            RazaoSocial = fornecedor.RazaoSocial,
            NomeFantasia = fornecedor.NomeFantasia,
            CnpjCpf = fornecedor.CnpjCpf.Valor,
            Email = fornecedor.Email.Valor,
            Telefone = fornecedor.Telefone,
            Tipo = fornecedor.Tipo.ToString(),
            Status = fornecedor.Status.ToString(),
            ContatoPrincipal = fornecedor.ContatoPrincipal,
            UltimaCompra = fornecedor.UltimaCompra,
            TotalComprado = fornecedor.TotalComprado,
            QuantidadeCompras = fornecedor.QuantidadeCompras,
            EhFrequente = fornecedor.EhFrequente(),
            Ativo = fornecedor.Ativo
        };
    }

    private async Task InvalidarCacheFornecedores()
    {
        await _cacheService.RemoveAsync("gp:fornecedores:all");
        await _cacheService.RemoveAsync("gp:fornecedores:list");
        await _cacheService.RemoveAsync("gp:fornecedores:frequentes");
    }
}