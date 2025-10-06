using Xunit;
using Moq;
using FluentAssertions;
using GestaoProdutos.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace GestaoProdutos.Tests.Unit.Services;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _cacheService = new RedisCacheService(_mockDistributedCache.Object, _mockLogger.Object);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_DeveRetornarObjetoQuandoExistir()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testObject, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        var bytes = Encoding.UTF8.GetBytes(json);

        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

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
        // Arrange
        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<TestCacheObject>("test:key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_DeveRetornarNullQuandoOcorrerErro()
    {
        // Arrange
        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

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
        byte[]? capturedBytes = null;

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                "test:key",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, bytes, options, token) => capturedBytes = bytes)
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync("test:key", testObject, TimeSpan.FromMinutes(30));

        // Assert
        _mockDistributedCache.Verify(x => x.SetAsync(
            "test:key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedBytes.Should().NotBeNull();
        var json = Encoding.UTF8.GetString(capturedBytes!);
        json.Should().Contain("\"id\":1");
        json.Should().Contain("\"name\":\"Test\"");
    }

    [Fact]
    public async Task SetAsync_DeveUsarExpiracaoPadrao()
    {
        // Arrange
        var testObject = new TestCacheObject { Id = 1, Name = "Test" };
        DistributedCacheEntryOptions? capturedOptions = null;

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, bytes, options, token) => capturedOptions = options)
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync("test:key", testObject);

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task SetAsync_DeveTratarErrosGraciosamente()
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

        // Act
        Func<Task> act = async () => await _cacheService.SetAsync("test:key", testObject);

        // Assert - não deve lançar exceção
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_DeveRemoverChaveComSucesso()
    {
        // Arrange
        _mockDistributedCache
            .Setup(x => x.RemoveAsync("test:key", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.RemoveAsync("test:key");

        // Assert
        _mockDistributedCache.Verify(x => x.RemoveAsync("test:key", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_DeveTratarErrosGraciosamente()
    {
        // Arrange
        _mockDistributedCache
            .Setup(x => x.RemoveAsync("test:key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act
        Func<Task> act = async () => await _cacheService.RemoveAsync("test:key");

        // Assert - não deve lançar exceção
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_DeveRetornarTrueQuandoChaveExiste()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("{}");
        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _cacheService.ExistsAsync("test:key");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_DeveRetornarFalseQuandoChaveNaoExiste()
    {
        // Arrange
        _mockDistributedCache
            .Setup(x => x.GetAsync("test:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

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

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetMultipleAsync(items, TimeSpan.FromMinutes(15));

        // Assert
        _mockDistributedCache.Verify(
            x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region GetMultipleAsync Tests

    [Fact]
    public async Task GetMultipleAsync_DeveRetornarMultiplosItens()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var obj1 = new TestCacheObject { Id = 1, Name = "Test1" };
        var obj2 = new TestCacheObject { Id = 2, Name = "Test2" };

        _mockDistributedCache
            .Setup(x => x.GetAsync("key1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj1, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })));

        _mockDistributedCache
            .Setup(x => x.GetAsync("key2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj2, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            })));

        // Act
        var result = await _cacheService.GetMultipleAsync<TestCacheObject>(keys);

        // Assert
        result.Should().HaveCount(2);
        result["key1"].Should().NotBeNull();
        result["key1"]!.Id.Should().Be(1);
        result["key2"].Should().NotBeNull();
        result["key2"]!.Id.Should().Be(2);
    }

    #endregion

    #region IncrementAsync Tests

    [Fact]
    public async Task IncrementAsync_DeveIncrementarContador()
    {
        // Arrange
        var currentValue = 5L;
        var bytes = Encoding.UTF8.GetBytes(currentValue.ToString());

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

        // Act
        var result = await _cacheService.IncrementAsync("counter:key", 3);

        // Assert
        result.Should().Be(8);
    }

    [Fact]
    public async Task IncrementAsync_DeveCriarContadorSeNaoExistir()
    {
        // Arrange
        _mockDistributedCache
            .Setup(x => x.GetAsync("counter:key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _mockDistributedCache
            .Setup(x => x.SetAsync(
                "counter:key",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cacheService.IncrementAsync("counter:key", 1);

        // Assert
        result.Should().Be(1);
    }

    #endregion

    // Classe auxiliar para testes
    private class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
