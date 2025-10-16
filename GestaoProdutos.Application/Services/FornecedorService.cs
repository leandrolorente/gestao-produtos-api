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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<FornecedorService> _logger;

    public FornecedorService(
        IFornecedorRepository fornecedorRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<FornecedorService> logger)
    {
        _fornecedorRepository = fornecedorRepository;
        _unitOfWork = unitOfWork;
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
                return cached;
            }

            // Buscar do banco
            var fornecedores = await _fornecedorRepository.GetAllAsync();
            var resultado = fornecedores.Select(MapToDto).ToList();

            // Salvar no cache (5 minutos)
            await _cacheService.SetAsync(cacheKey, resultado, TimeSpan.FromMinutes(5));
            
            _logger.LogInformation("🗄️ [DATABASE] Fornecedores buscados no MongoDB e salvos no cache");

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

            var resultado = await MapToDtoAsync(fornecedor);
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

            // Criar o endereço primeiro se fornecido
            string? enderecoId = null;
            if (dto.Endereco != null)
            {
                var endereco = new EnderecoEntity
                {
                    Cep = dto.Endereco.Cep,
                    Logradouro = dto.Endereco.Logradouro,
                    Numero = dto.Endereco.Numero,
                    Complemento = dto.Endereco.Complemento ?? string.Empty,
                    Unidade = dto.Endereco.Unidade,
                    Bairro = dto.Endereco.Bairro,
                    Localidade = dto.Endereco.Localidade,
                    Uf = dto.Endereco.Uf,
                    Estado = dto.Endereco.Estado,
                    Regiao = dto.Endereco.Regiao,
                    Referencia = dto.Endereco.Referencia,
                    IsPrincipal = dto.Endereco.IsPrincipal,
                    Tipo = dto.Endereco.Tipo.ToString()
                };

                var enderecoCreated = await _unitOfWork.Enderecos.CreateAsync(endereco);
                enderecoId = enderecoCreated.Id;
            }

            // Mapear DTO para entidade
            var fornecedor = new Fornecedor
            {
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                CnpjCpf = new CpfCnpj(dto.CnpjCpf),
                Email = new Email(dto.Email),
                Telefone = dto.Telefone,
                EnderecoId = enderecoId, // Usar a referência ao endereço criado
                InscricaoEstadual = dto.InscricaoEstadual,
                InscricaoMunicipal = dto.InscricaoMunicipal,
                Tipo = dto.Tipo,
                Status = dto.Status ?? StatusFornecedor.Ativo, // Usar status do DTO ou padrão Ativo
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

            var fornecedorCriado = await _fornecedorRepository.CreateAsync(fornecedor);
            await _unitOfWork.SaveChangesAsync();

            // Invalidar cache
            await InvalidarCacheFornecedores();

            _logger.LogInformation("Fornecedor criado com sucesso: {Id} - {RazaoSocial}", fornecedorCriado.Id, fornecedorCriado.RazaoSocial);

            return await MapToDtoAsync(fornecedorCriado);
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

            // Atualizar ou criar endereço se dto.Endereco for fornecido
            if (dto.Endereco != null)
            {
                if (!string.IsNullOrEmpty(fornecedor.EnderecoId))
                {
                    // Fornecedor já tem endereço - atualizar
                    var endereco = await _unitOfWork.Enderecos.GetByIdAsync(fornecedor.EnderecoId);
                    if (endereco != null)
                    {
                        Console.WriteLine($"Atualizando endereço existente para fornecedor {fornecedor.RazaoSocial}");
                        endereco.Cep = dto.Endereco.Cep;
                        endereco.Logradouro = dto.Endereco.Logradouro;
                        endereco.Numero = dto.Endereco.Numero;
                        endereco.Complemento = dto.Endereco.Complemento ?? string.Empty;
                        endereco.Unidade = dto.Endereco.Unidade;
                        endereco.Bairro = dto.Endereco.Bairro;
                        endereco.Localidade = dto.Endereco.Localidade;
                        endereco.Uf = dto.Endereco.Uf;
                        endereco.Estado = dto.Endereco.Estado;
                        endereco.Regiao = dto.Endereco.Regiao;
                        endereco.Referencia = dto.Endereco.Referencia;
                        endereco.IsPrincipal = dto.Endereco.IsPrincipal;
                        endereco.Tipo = dto.Endereco.Tipo.ToString();

                        await _unitOfWork.Enderecos.UpdateAsync(endereco);
                    }
                }
                else
                {
                    // Fornecedor não tem endereço - criar novo
                    Console.WriteLine($"Criando novo endereço para fornecedor {fornecedor.RazaoSocial}");
                    var novoEndereco = new EnderecoEntity
                    {
                        Cep = dto.Endereco.Cep,
                        Logradouro = dto.Endereco.Logradouro,
                        Numero = dto.Endereco.Numero,
                        Complemento = dto.Endereco.Complemento ?? string.Empty,
                        Unidade = dto.Endereco.Unidade,
                        Bairro = dto.Endereco.Bairro,
                        Localidade = dto.Endereco.Localidade,
                        Uf = dto.Endereco.Uf,
                        Estado = dto.Endereco.Estado,
                        Regiao = dto.Endereco.Regiao,
                        Referencia = dto.Endereco.Referencia,
                        IsPrincipal = dto.Endereco.IsPrincipal,
                        Tipo = dto.Endereco.Tipo.ToString()
                    };

                    await _unitOfWork.Enderecos.CreateAsync(novoEndereco);
                    fornecedor.EnderecoId = novoEndereco.Id;
                    Console.WriteLine($"Endereço criado com ID: {novoEndereco.Id} para fornecedor {fornecedor.RazaoSocial}");
                }
            }

            // Atualizar dados básicos
            fornecedor.AtualizarDados(dto.RazaoSocial, dto.NomeFantasia, dto.Telefone, dto.ContatoPrincipal);
            
            // Atualizar email
            fornecedor.Email = new Email(dto.Email);
            
            // Atualizar outros campos
            fornecedor.InscricaoEstadual = dto.InscricaoEstadual;
            fornecedor.InscricaoMunicipal = dto.InscricaoMunicipal;
            if (dto.Tipo.HasValue)
                fornecedor.Tipo = dto.Tipo.Value;
            fornecedor.Status = dto.Status;
            fornecedor.Observacoes = dto.Observacoes;
            fornecedor.Site = dto.Site;

            // Atualizar condições comerciais
            fornecedor.AtualizarCondicoesComerciais(dto.PrazoPagamentoPadrao, dto.LimiteCredito, dto.CondicoesPagamento);
            
            // Atualizar dados bancários
            fornecedor.AtualizarDadosBancarios(dto.Banco, dto.Agencia, dto.Conta, dto.Pix);

            var fornecedorAtualizado = await _fornecedorRepository.UpdateAsync(fornecedor);
            await _unitOfWork.SaveChangesAsync();

            // Invalidar cache
            await InvalidarCacheFornecedores();
            await _cacheService.RemoveAsync($"gp:fornecedor:{id}");

            _logger.LogInformation("Fornecedor atualizado com sucesso: {Id} - {RazaoSocial}", id, fornecedor.RazaoSocial);

            return await MapToDtoAsync(fornecedorAtualizado);
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

    private async Task<FornecedorDto> MapToDtoAsync(Fornecedor fornecedor)
    {
        var enderecoDto = await CarregarEnderecoAsync(fornecedor.EnderecoId);
        
        return new FornecedorDto
        {
            Id = fornecedor.Id,
            RazaoSocial = fornecedor.RazaoSocial,
            NomeFantasia = fornecedor.NomeFantasia,
            CnpjCpf = fornecedor.CnpjCpf.Valor,
            Email = fornecedor.Email.Valor,
            Telefone = fornecedor.Telefone,
            Endereco = enderecoDto,
            InscricaoEstadual = fornecedor.InscricaoEstadual,
            InscricaoMunicipal = fornecedor.InscricaoMunicipal,
            Tipo = ((int)fornecedor.Tipo).ToString(),
            Status = ((int)fornecedor.Status).ToString(),
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
            QuantidadeProdutos = fornecedor.ProdutoIds?.Count ?? 0,
            UltimaCompra = fornecedor.UltimaCompra,
            TotalComprado = fornecedor.TotalComprado,
            QuantidadeCompras = fornecedor.QuantidadeCompras,
            TicketMedio = fornecedor.QuantidadeCompras > 0 ? fornecedor.TotalComprado / fornecedor.QuantidadeCompras : 0,
            EhFrequente = fornecedor.QuantidadeCompras >= 5,
            DataCriacao = fornecedor.DataCriacao,
            DataAtualizacao = fornecedor.DataAtualizacao,
            Ativo = fornecedor.Ativo
        };
    }

    private async Task<EnderecoDto?> CarregarEnderecoAsync(string? enderecoId)
    {
        if (string.IsNullOrEmpty(enderecoId))
            return null;

        var endereco = await _unitOfWork.Enderecos.GetByIdAsync(enderecoId);
        if (endereco == null)
            return null;

        return new EnderecoDto
        {
            Id = endereco.Id,
            Logradouro = endereco.Logradouro,
            Numero = endereco.Numero,
            Complemento = endereco.Complemento,
            Unidade = endereco.Unidade,
            Bairro = endereco.Bairro,
            Localidade = endereco.Localidade,
            Uf = endereco.Uf,
            Estado = endereco.Estado,
            Regiao = endereco.Regiao,
            Referencia = endereco.Referencia,
            Cep = endereco.Cep,
            IsPrincipal = endereco.IsPrincipal,
            Tipo = endereco.Tipo,
            Ativo = endereco.Ativo,
            DataCriacao = endereco.DataCriacao,
            DataAtualizacao = endereco.DataAtualizacao
        };
    }

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
                Id = string.Empty, // ValueObject não tem ID
                Logradouro = fornecedor.Endereco.Logradouro,
                Numero = string.Empty, // ValueObject não tem Numero
                Complemento = fornecedor.Endereco.Complemento ?? string.Empty,
                Unidade = string.Empty, // ValueObject não tem Unidade
                Bairro = string.Empty, // ValueObject não tem Bairro
                Localidade = fornecedor.Endereco.Cidade,
                Uf = string.Empty, // ValueObject não tem UF separado
                Estado = fornecedor.Endereco.Estado,
                Regiao = string.Empty, // ValueObject não tem Regiao
                Referencia = string.Empty, // ValueObject não tem Referencia
                Cep = fornecedor.Endereco.Cep,
                IsPrincipal = true, // Assumir como principal
                Tipo = "Comercial", // Assumir como comercial para fornecedor
                Ativo = true,
                DataCriacao = fornecedor.DataCriacao,
                DataAtualizacao = fornecedor.DataAtualizacao
            } : null,
            InscricaoEstadual = fornecedor.InscricaoEstadual,
            InscricaoMunicipal = fornecedor.InscricaoMunicipal,
            Tipo = ((int)fornecedor.Tipo).ToString(),
            Status = ((int)fornecedor.Status).ToString(),
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
