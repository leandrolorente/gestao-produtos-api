using Xunit;
using Moq;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GestaoProdutos.Tests.Unit.Services;

public class MemoryCacheServiceTests
{
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<ILogger<MemoryCacheService>> _mockLogger;
    private readonly MemoryCacheService _cacheService;
    private readonly Dictionary<string, object> _cacheStore;

    public MemoryCacheServiceTests()
    {
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<MemoryCacheService>>();
        _cacheStore = new Dictionary<string, object>();

        // Configurar mock para simular comportamento de cache
        _mockMemoryCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
            .Returns((object key, out object value) =>
            {
                var keyStr = key.ToString()!;
                if (_cacheStore.ContainsKey(keyStr))
                {
                    value = _cacheStore[keyStr];
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
                            _cacheStore[keyStr] = mockEntry.Object.Value;
                        }
                    });

                return mockEntry.Object;
            });

        _mockMemoryCache
            .Setup(x => x.Remove(It.IsAny<object>()))
            .Callback<object>(key =>
            {
                var keyStr = key.ToString()!;
                _cacheStore.Remove(keyStr);
            });

        _cacheService = new MemoryCacheService(_mockMemoryCache.Object, _mockLogger.Object);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_DeveRetornarObjetoQuandoExistir()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        _cacheStore["test:key"] = testObject;

        // Act
        var result = await _cacheService.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_DeveRetornarNullQuandoNaoExistir()
    {
        // Arrange - cache vazio

        // Act
        var result = await _cacheService.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_DeveArmazenarObjetoComSucesso()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync("test:key", testObject, TimeSpan.FromMinutes(30));

        // Assert
        _cacheStore.Should().ContainKey("test:key");
        var storedJson = _cacheStore["test:key"] as string;
        storedJson.Should().NotBeNull();
        
        var stored = System.Text.Json.JsonSerializer.Deserialize<TestCacheObject>(storedJson!);
        stored.Should().NotBeNull();
        stored!.Id.Should().Be(1);
    }

    [Fact]
    public async Task SetAsync_DeveUsarExpiracaoPadrao()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        MemoryCacheEntryOptions? capturedOptions = null;

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
                        capturedOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = mockEntry.Object.AbsoluteExpirationRelativeToNow
                        };
                    });

                return mockEntry.Object;
            });

        // Act
        await _cacheService.SetAsync("test:key", testObject);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(30));
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_DeveRemoverChaveExistente()
    {
        // Arrange
        _cacheStore["test:key"] = new TestCacheObject { Id = 1, Name = "Test" };

        // Act
        await _cacheService.RemoveAsync("test:key");

        // Assert
        _cacheStore.Should().NotContainKey("test:key");
    }

    [Fact]
    public async Task RemoveAsync_NaoDeveFalharQuandoChaveNaoExiste()
    {
        // Arrange - cache vazio

        // Act
        Func<Task> act = async () => await _cacheService.RemoveAsync("test:key");

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_DeveRetornarTrueQuandoChaveExiste()
    {
        // Arrange
        _cacheStore["test:key"] = new TestCacheObject { Id = 1, Name = "Test" };

        // Act
        var result = await _cacheService.ExistsAsync("test:key");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_DeveRetornarFalseQuandoChaveNaoExiste()
    {
        // Arrange - cache vazio

        // Act
        var result = await _cacheService.ExistsAsync("test:key");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SetMultipleAsync Tests

    [Fact]
    public async Task SetMultipleAsync_DeveArmazenarMultiplosItens()
    {
        // Arrange
        var items = new Dictionary<string, TestCacheObject>
        {
            { "key1", new TestCacheObject { Id = 1, Name = "Test1" } },
            { "key2", new TestCacheObject { Id = 2, Name = "Test2" } }
        };

        // Act
        await _cacheService.SetMultipleAsync(items, TimeSpan.FromMinutes(15));

        // Assert
        _cacheStore.Should().ContainKey("key1");
        _cacheStore.Should().ContainKey("key2");
        
        var obj1Json = _cacheStore["key1"] as string;
        obj1Json.Should().NotBeNull();
        var obj1 = System.Text.Json.JsonSerializer.Deserialize<TestCacheObject>(obj1Json!);
        obj1.Should().NotBeNull();
        obj1!.Id.Should().Be(1);

        var obj2Json = _cacheStore["key2"] as string;
        obj2Json.Should().NotBeNull();
        var obj2 = System.Text.Json.JsonSerializer.Deserialize<TestCacheObject>(obj2Json!);
        obj2.Should().NotBeNull();
        obj2!.Id.Should().Be(2);
    }

    #endregion

    #region GetMultipleAsync Tests

    [Fact]
    public async Task GetMultipleAsync_DeveRetornarMultiplosItens()
    {
        // Arrange
        _cacheStore["key1"] = new TestCacheObject { Id = 1, Name = "Test1" };
        _cacheStore["key2"] = new TestCacheObject { Id = 2, Name = "Test2" };
        var keys = new[] { "key1", "key2", "key3" }; // key3 não existe

        // Act
        var result = await _cacheService.GetMultipleAsync<TestCacheObject>(keys);

        // Assert
        result.Should().HaveCount(3);
        result["key1"].Should().NotBeNull();
        result["key1"]!.Id.Should().Be(1);
        result["key2"].Should().NotBeNull();
        result["key2"]!.Id.Should().Be(2);
        result["key3"].Should().BeNull();
    }

    #endregion

    #region IncrementAsync Tests

    [Fact]
    public async Task IncrementAsync_DeveIncrementarContadorExistente()
    {
        // Arrange
        _cacheStore["counter:key"] = 5L;

        // Act
        var result = await _cacheService.IncrementAsync("counter:key", 3);

        // Assert
        result.Should().Be(8);
        _cacheStore["counter:key"].Should().Be(8L);
    }

    [Fact]
    public async Task IncrementAsync_DeveCriarContadorSeNaoExistir()
    {
        // Arrange - cache vazio

        // Act
        var result = await _cacheService.IncrementAsync("counter:key", 1);

        // Assert
        result.Should().Be(1);
        _cacheStore.Should().ContainKey("counter:key");
    }

    [Fact]
    public async Task IncrementAsync_DeveSuportarIncrementosNegativos()
    {
        // Arrange
        _cacheStore["counter:key"] = 10L;

        // Act
        var result = await _cacheService.IncrementAsync("counter:key", -3);

        // Assert
        result.Should().Be(7);
    }

    #endregion

    #region RemovePatternAsync Tests

    [Fact]
    public async Task RemovePatternAsync_NaoEhSuportadoEmMemoryCache()
    {
        // Arrange
        _cacheStore["produtos:1"] = new TestCacheObject { Id = 1, Name = "Test1" };
        _cacheStore["produtos:2"] = new TestCacheObject { Id = 2, Name = "Test2" };

        // Act
        await _cacheService.RemovePatternAsync("produtos:*");

        // Assert - não remove porque não é suportado em Memory Cache
        _cacheStore.Should().ContainKey("produtos:1");
        _cacheStore.Should().ContainKey("produtos:2");
    }

    #endregion

    // Classe auxiliar para testes
    private class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
