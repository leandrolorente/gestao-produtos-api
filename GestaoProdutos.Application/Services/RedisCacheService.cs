using GestaoProdutos.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GestaoProdutos.Application.Services
{
    /// <summary>
    /// Implementação Redis usando IDistributedCache (funciona com Redis real)
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
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
                var cachedValue = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug($"❌ [REDIS MISS] Chave não encontrada: {key}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
                _logger.LogDebug($"✅ [REDIS HIT] Dados encontrados no cache: {key}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao obter do Redis: {key} - {ex.Message}");
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue)
                    options.SetAbsoluteExpiration(expiry.Value);
                else
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Padrão 30 min

                await _distributedCache.SetStringAsync(key, jsonValue, options);
                _logger.LogDebug($"💾 [REDIS SET] Dados salvos no cache: {key} (TTL: {expiry?.TotalMinutes ?? 30:F0} min)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao definir no Redis: {key} - {ex.Message}");
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug($"✅ Cache REMOVE: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao remover do Redis: {key} - {ex.Message}");
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                // IDistributedCache não suporta pattern removal diretamente
                // Para implementação completa, precisaríamos do StackExchange.Redis direto
                _logger.LogWarning($"⚠️ Pattern removal não suportado com IDistributedCache: {pattern}");
                
                // Por enquanto, implementação básica para padrões conhecidos
                if (pattern.StartsWith("produtos:"))
                {
                    _logger.LogDebug($"Pattern detectado para produtos: {pattern}");
                    // Aqui você implementaria a lógica específica se necessário
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao remover pattern: {pattern} - {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao verificar existência: {key} - {ex.Message}");
                return false;
            }
        }

        public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var tasks = items.Select(kvp => SetAsync(kvp.Key, kvp.Value, expiry));
                await Task.WhenAll(tasks);
                _logger.LogDebug($"✅ Cache SET Multiple: {items.Count} itens");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao definir múltiplos itens: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class
        {
            var result = new Dictionary<string, T?>();
            
            try
            {
                var tasks = keys.Select(async key => new { Key = key, Value = await GetAsync<T>(key) });
                var results = await Task.WhenAll(tasks);
                
                foreach (var item in results)
                {
                    result[item.Key] = item.Value;
                }
                
                _logger.LogDebug($"✅ Cache GET Multiple: {result.Count} itens");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao obter múltiplos itens: {ex.Message}");
            }
            
            return result;
        }

        public async Task<long> IncrementAsync(string key, long increment = 1)
        {
            try
            {
                // Implementação simples usando GET/SET
                // Para performance real, usar StackExchange.Redis direto
                var cached = await _distributedCache.GetStringAsync(key);
                var currentValue = string.IsNullOrEmpty(cached) ? 0 : long.Parse(cached);
                var newValue = currentValue + increment;
                
                await _distributedCache.SetStringAsync(key, newValue.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });
                
                _logger.LogDebug($"✅ Cache INCREMENT: {key} = {newValue}");
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao incrementar: {key} - {ex.Message}");
                return 0;
            }
        }

        public async Task SetExpiryAsync(string key, TimeSpan expiry)
        {
            try
            {
                // IDistributedCache não permite definir TTL em chave existente
                // Precisaríamos recriar a entrada
                var value = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(value))
                {
                    await _distributedCache.RemoveAsync(key);
                    await _distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiry
                    });
                    _logger.LogDebug($"✅ TTL atualizado: {key} = {expiry.TotalMinutes} min");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Erro ao definir TTL: {key} - {ex.Message}");
            }
        }
    }
}
