using GestaoProdutos.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GestaoProdutos.Application.Services
{
    /// <summary>
    /// Implementação de cache usando Memory Cache (fallback quando Redis não disponível)
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cached = _memoryCache.Get(key);
                if (cached == null) return default;

                if (cached is T directValue)
                    return directValue;

                if (cached is string jsonString)
                    return JsonSerializer.Deserialize<T>(jsonString, _jsonOptions);

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao obter do cache: {key} - {ex.Message}");
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiry.HasValue)
                    options.SetAbsoluteExpiration(expiry.Value);
                else
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Padrão 30 min

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                _memoryCache.Set(key, serializedValue, options);
                
                _logger.LogDebug($"Cache definido: {key} (expira em {expiry?.TotalMinutes ?? 30} min)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao definir cache: {key} - {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug($"Cache removido: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao remover cache: {key} - {ex.Message}");
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                // Memory Cache não suporte pattern, então vamos simular
                _logger.LogInformation($"Pattern removal não suportado em MemoryCache: {pattern}");
                
                // Se for um padrão específico que conhecemos, podemos implementar
                if (pattern.Contains("produtos:"))
                {
                    // Aqui poderíamos manter uma lista de chaves para remoção
                    _logger.LogDebug($"Pattern específico detectado para produtos: {pattern}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao remover pattern: {pattern} - {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao verificar existência: {key} - {ex.Message}");
                return false;
            }
        }

        public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class
        {
            try
            {
                foreach (var item in items)
                {
                    await SetAsync(item.Key, item.Value, expiry);
                }
                _logger.LogDebug($"Múltiplos itens definidos no cache: {items.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao definir múltiplos itens: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class
        {
            var result = new Dictionary<string, T?>();
            
            try
            {
                foreach (var key in keys)
                {
                    result[key] = await GetAsync<T>(key);
                }
                _logger.LogDebug($"Múltiplos itens recuperados do cache: {result.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao recuperar múltiplos itens: {ex.Message}");
            }
            
            return result;
        }

        public async Task<long> IncrementAsync(string key, long increment = 1)
        {
            try
            {
                // Para Memory Cache, vamos simular um contador
                var currentValue = _memoryCache.Get<long?>(key) ?? 0;
                var newValue = currentValue + increment;
                
                _memoryCache.Set(key, newValue, TimeSpan.FromHours(24)); // Contadores duram 24h
                
                _logger.LogDebug($"Contador incrementado: {key} = {newValue}");
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao incrementar contador: {key} - {ex.Message}");
                return 0;
            }
        }

        public async Task SetExpiryAsync(string key, TimeSpan expiry)
        {
            try
            {
                // Memory Cache não permite alterar TTL de chave existente
                // Vamos re-criar a entrada se ela existir
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    _memoryCache.Remove(key);
                    _memoryCache.Set(key, value, expiry);
                    _logger.LogDebug($"TTL atualizado para chave: {key} = {expiry.TotalMinutes} min");
                }
                else
                {
                    _logger.LogWarning($"Tentativa de definir TTL para chave inexistente: {key}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro ao definir TTL: {key} - {ex.Message}");
            }
        }
    }
}