using GestaoProdutos.Application.DTOs;
using GestaoProdutos.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GestaoProdutos.Application.Services;

/// <summary>
/// Serviço para integração com a API do ViaCEP com cache Redis
/// </summary>
public class ViaCepService : IViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ICacheService _cache;
    private readonly ILogger<ViaCepService> _logger;
    private const string VIACEP_BASE_URL = "https://viacep.com.br/ws/";

    public ViaCepService(
        HttpClient httpClient, 
        ICacheService cache,
        ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Busca informações de endereço pelo CEP com cache de 24 horas
    /// </summary>
    /// <param name="cep">CEP para consulta</param>
    /// <returns>Dados do endereço filtrados</returns>
    public async Task<ViaCepResponseDto?> BuscarEnderecoPorCepAsync(string cep)
    {
        try
        {
            // Validar e formatar CEP
            var cepLimpo = LimparCep(cep);
            if (!ValidarCep(cepLimpo))
            {
                _logger.LogWarning("CEP inválido fornecido: {Cep}", cep);
                return null;
            }

            // Verificar cache primeiro
            var cacheKey = $"gp:viacep:{cepLimpo}";
            var cachedResult = await _cache.GetAsync<ViaCepResponseDto>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Endereço para CEP {Cep} recuperado do cache", cepLimpo);
                return cachedResult;
            }

            // Fazer requisição para ViaCEP
            var url = $"{VIACEP_BASE_URL}{cepLimpo}/json/";
            _logger.LogInformation("Consultando ViaCEP para CEP: {Cep}", cepLimpo);

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro na requisição ViaCEP. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var viaCepResponse = JsonSerializer.Deserialize<ViaCepFullResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Verificar se o CEP foi encontrado (ViaCEP retorna erro: true quando não encontra)
            if (viaCepResponse == null || viaCepResponse.Erro)
            {
                _logger.LogWarning("CEP não encontrado: {Cep}", cepLimpo);
                return null;
            }

            // Mapear para DTO filtrado
            var resultado = new ViaCepResponseDto
            {
                Cep = viaCepResponse.Cep,
                Logradouro = viaCepResponse.Logradouro,
                Complemento = viaCepResponse.Complemento,
                Unidade = viaCepResponse.Unidade,
                Bairro = viaCepResponse.Bairro,
                Localidade = viaCepResponse.Localidade,
                Uf = viaCepResponse.Uf,
                Estado = viaCepResponse.Estado,
                Regiao = viaCepResponse.Regiao
            };

            // Armazenar no cache por 24 horas (endereços não mudam frequentemente)
            await _cache.SetAsync(cacheKey, resultado, TimeSpan.FromHours(24));
            _logger.LogDebug("Endereço para CEP {Cep} armazenado no cache por 24 horas", cepLimpo);

            _logger.LogInformation("CEP encontrado com sucesso: {Cep} - {Localidade}/{Uf}", 
                resultado.Cep, resultado.Localidade, resultado.Uf);

            return resultado;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de conexão ao consultar ViaCEP para CEP: {Cep}", cep);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout ao consultar ViaCEP para CEP: {Cep}", cep);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar resposta do ViaCEP para CEP: {Cep}", cep);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao consultar ViaCEP para CEP: {Cep}", cep);
            return null;
        }
    }

    /// <summary>
    /// Remove caracteres não numéricos do CEP
    /// </summary>
    /// <param name="cep">CEP para limpar</param>
    /// <returns>CEP apenas com números</returns>
    private static string LimparCep(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return string.Empty;

        return Regex.Replace(cep, @"[^\d]", "");
    }

    /// <summary>
    /// Valida se o CEP possui formato correto (8 dígitos)
    /// </summary>
    /// <param name="cep">CEP para validar</param>
    /// <returns>True se válido, false caso contrário</returns>
    private static bool ValidarCep(string cep)
    {
        return !string.IsNullOrWhiteSpace(cep) && 
               cep.Length == 8 && 
               cep.All(char.IsDigit);
    }
}