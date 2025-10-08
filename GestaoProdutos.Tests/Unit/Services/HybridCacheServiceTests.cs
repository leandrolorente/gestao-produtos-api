using Xunit;
using Moq;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using GestaoProdutos.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Tests.Unit.Services;

public class HybridCacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<ILogger<HybridCacheService>> _mockLogger;
    private readonly Mock<ILogger<RedisCacheService>> _mockRedisLogger;
    private readonly Mock<ILogger<MemoryCacheService>> _mockMemoryLogger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, object> _memoryCacheStore;

    public HybridCacheServiceTests()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<HybridCacheService>>();
        _mockRedisLogger = new Mock<ILogger<RedisCacheService>>();
        _mockMemoryLogger = new Mock<ILogger<MemoryCacheService>>();
        _memoryCacheStore = new Dictionary<string, object>();

        // Configurar Memory Cache mock
        ConfigureMemoryCacheMock();

        // Configurar Service Provider
        var services = new ServiceCollection();
        services.AddSingleton(_mockDistributedCache.Object);
        services.AddSingleton(_mockMemoryCache.Object);
        services.AddSingleton(_mockRedisLogger.Object);
        services.AddSingleton(_mockMemoryLogger.Object);
        services.AddScoped<RedisCacheService>();
        services.AddScoped<MemoryCacheService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMemoryCacheMock()
    {
        _mockMemoryCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
            .Returns((object key, out object value) =>
            {
                var keyStr = key.ToString()!;
                if (_memoryCacheStore.ContainsKey(keyStr))
                {
                    value = _memoryCacheStore[keyStr];
                    return true;
                }
                value = null!;
                return false;
            });

        _mockMemoryCache
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns((object key) =>
            {
                var mockEntry = new Mock<ICacheEntry>();
                mockEntry.SetupProperty(e => e.Value);
                mockEntry.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
                
                mockEntry
                    .Setup(e => e.Dispose())
                    .Callback(() =>
                    {
                        var keyStr = key.ToString()!;
                        if (mockEntry.Object.Value != null)
                        {
                            _memoryCacheStore[keyStr] = mockEntry.Object.Value;
                        }
                    });

                return mockEntry.Object;
            });

        _mockMemoryCache
            .Setup(x => x.Remove(It.IsAny<object>()))
            .Callback<object>(key =>
            {
                var keyStr = key.ToString()!;
                _memoryCacheStore.Remove(keyStr);
            });
    }

    #region Redis Disponível Tests

    [Fact]
    public async Task GetAsync_DeveUsarRedisQuandoDisponivel()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var json = System.Text.Json.JsonSerializer.Serialize(testObject, new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        _mockDistributedCache.Verify(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_DeveUsarRedisQuandoDisponivel()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                "test:key",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        await hybridCache.SetAsync("test:key", testObject, TimeSpan.FromMinutes(30));

        // Assert
        _mockDistributedCache.Verify(x => x.SetAsync(
            "test:key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Fallback para Memory Cache Tests

    [Fact]
    public async Task GetAsync_DeveFazerFallbackParaMemoryCacheQuandoRedisFalhar()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(testObject);
        _memoryCacheStore["test:key"] = jsonString;

        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task SetAsync_DeveFazerFallbackParaMemoryCacheQuandoRedisFalhar()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        await hybridCache.SetAsync("test:key", testObject, TimeSpan.FromMinutes(30));

        // Assert
        _memoryCacheStore.Should().ContainKey("test:key");
        var storedJson = _memoryCacheStore["test:key"] as string;
        storedJson.Should().NotBeNull();
        var stored = System.Text.Json.JsonSerializer.Deserialize<TestCacheObject>(storedJson!);
        stored.Should().NotBeNull();
        stored!.Id.Should().Be(1);
    }

    [Fact]
    public async Task RemoveAsync_DeveFazerFallbackParaMemoryCacheQuandoRedisFalhar()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(testObject);
        _memoryCacheStore["test:key"] = jsonString;

        _mockDistributedCache
            .Setup(x => x.RemoveAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        await hybridCache.RemoveAsync("test:key");

        // Assert
        _memoryCacheStore.Should().NotContainKey("test:key");
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_DeveRetornarTrueQuandoChaveExisteNoRedis()
    {
        // Arrange
        var bytes = System.Text.Encoding.UTF8.GetBytes("{}");
        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.ExistsAsync("test:key");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_DeveRetornarTrueQuandoChaveExisteNoMemoryCacheAposFallback()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(testObject);
        _memoryCacheStore["test:key"] = jsonString;

        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.ExistsAsync("test:key");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region IncrementAsync Tests

    [Fact]
    public async Task IncrementAsync_DeveIncrementarContadorNoRedis()
    {
        // Arrange
        var currentValue = 5L;
        var bytes = System.Text.Encoding.UTF8.GetBytes(currentValue.ToString());

        _mockDistributedCache
            .Setup(x => x.GetAsync("counter:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                "counter:key",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.IncrementAsync("counter:key", 3);

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public async Task IncrementAsync_DeveFazerFallbackParaMemoryCacheQuandoRedisFalhar()
    {
        // Arrange
        var initialValue = 10L;
        var jsonString = System.Text.Json.JsonSerializer.Serialize(initialValue);
        _memoryCacheStore["counter:key"] = jsonString;

        _mockDistributedCache
            .Setup(x => x.GetAsync("counter:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.IncrementAsync("counter:key", 5);

        // Assert
        result.Should().Be(15);
        _memoryCacheStore["counter:key"].Should().Be(15L);
    }

    #endregion

    #region SetMultipleAsync e GetMultipleAsync Tests

    [Fact]
    public async Task SetMultipleAsync_DeveArmazenarMultiplosItensNoRedis()
    {
        // Arrange
        var items = new Dictionary<string, TestCacheObject>
        {
            { "key1", new TestCacheObject { Id = 1, Name = "Test1" } },
            { "key2", new TestCacheObject { Id = 2, Name = "Test2" } }
        };

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        await hybridCache.SetMultipleAsync(items, TimeSpan.FromMinutes(15));

        // Assert
        _mockDistributedCache.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetMultipleAsync_DeveRecuperarMultiplosItensDoRedis()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var obj1 = new TestCacheObject { Id = 1, Name = "Test1" };
        var obj2 = new TestCacheObject { Id = 2, Name = "Test2" };

        _mockDistributedCache
            .Setup(x => x.GetAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(obj1, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            })));

        _mockDistributedCache
            .Setup(x => x.GetAsync("key2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(obj2, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
            })));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.GetMultipleAsync<TestCacheObject>(keys);

        // Assert
        result.Should().HaveCount(2);
        result["key1"].Should().NotBeNull();
        result["key1"]!.Id.Should().Be(1);
        result["key2"].Should().NotBeNull();
        result["key2"]!.Id.Should().Be(2);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task GetAsync_DeveLogarQuandoRedisFalharEFizerFallback()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var jsonString = System.Text.Json.JsonSerializer.Serialize(testObject);
        _memoryCacheStore["test:key"] = jsonString;

        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var hybridCache = new HybridCacheService(
            _mockDistributedCache.Object,
            _mockMemoryCache.Object,
            _serviceProvider,
            _mockLogger.Object);

        // Act
        var result = await hybridCache.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().NotBeNull();
        // Verifica se o log de warning foi chamado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Redis falhou")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    // Classe auxiliar para testes
    private class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
