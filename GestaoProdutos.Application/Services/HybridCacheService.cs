using GestaoProdutos.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Application.Services;

public class HybridCacheService : ICacheService
{
    private readonly ICacheService _primaryCache = null!;
    private readonly ICacheService _fallbackCache;
    private readonly ILogger<HybridCacheService> _logger;
    private bool _redisAvailable = true;

    public HybridCacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        IServiceProvider serviceProvider,
        ILogger<HybridCacheService> logger)
    {
        _logger = logger;
        try
        {
            _primaryCache = serviceProvider.GetRequiredService<RedisCacheService>();
            _logger.LogInformation("Redis Cache configurado como primário");
            _redisAvailable = true;
        }
        catch
        {
            _logger.LogWarning("Redis não disponível, usando Memory Cache");
            _redisAvailable = false;
        }
        _fallbackCache = serviceProvider.GetRequiredService<MemoryCacheService>();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await ExecuteWithFallback(
            async () => await _primaryCache.GetAsync<T>(key),
            async () => await _fallbackCache.GetAsync<T>(key),
            nameof(GetAsync)
        );
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        await ExecuteWithFallback(
            async () => { await _primaryCache.SetAsync(key, value, expiry); return true; },
            async () => { await _fallbackCache.SetAsync(key, value, expiry); return true; },
            nameof(SetAsync)
        );
    }

    public async Task RemoveAsync(string key)
    {
        await ExecuteWithFallback(
            async () => { await _primaryCache.RemoveAsync(key); return true; },
            async () => { await _fallbackCache.RemoveAsync(key); return true; },
            nameof(RemoveAsync)
        );
    }

    public async Task RemovePatternAsync(string pattern)
    {
        await ExecuteWithFallback(
            async () => { await _primaryCache.RemovePatternAsync(pattern); return true; },
            async () => { await _fallbackCache.RemovePatternAsync(pattern); return true; },
            nameof(RemovePatternAsync)
        );
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await ExecuteWithFallback(
            async () => await _primaryCache.ExistsAsync(key),
            async () => await _fallbackCache.ExistsAsync(key),
            nameof(ExistsAsync)
        );
    }

    public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class
    {
        await ExecuteWithFallback(
            async () => { await _primaryCache.SetMultipleAsync(items, expiry); return true; },
            async () => { await _fallbackCache.SetMultipleAsync(items, expiry); return true; },
            nameof(SetMultipleAsync)
        );
    }

    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class
    {
        return await ExecuteWithFallback(
            async () => await _primaryCache.GetMultipleAsync<T>(keys),
            async () => await _fallbackCache.GetMultipleAsync<T>(keys),
            nameof(GetMultipleAsync)
        );
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        return await ExecuteWithFallback(
            async () => await _primaryCache.IncrementAsync(key, value),
            async () => await _fallbackCache.IncrementAsync(key, value),
            nameof(IncrementAsync)
        );
    }

    public async Task SetExpiryAsync(string key, TimeSpan expiry)
    {
        await ExecuteWithFallback(
            async () => { await _primaryCache.SetExpiryAsync(key, expiry); return true; },
            async () => { await _fallbackCache.SetExpiryAsync(key, expiry); return true; },
            nameof(SetExpiryAsync)
        );
    }

    private async Task<T> ExecuteWithFallback<T>(
        Func<Task<T>> primaryAction,
        Func<Task<T>> fallbackAction,
        string operationName)
    {
        if (_redisAvailable)
        {
            try
            {
                return await primaryAction();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis falhou em {Operation}, usando fallback", operationName);
                _redisAvailable = false;
                return await fallbackAction();
            }
        }
        return await fallbackAction();
    }
}
