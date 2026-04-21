using FinanceControl.Api.Infra;
using StackExchange.Redis;
using Xunit;

namespace FinanceControl.IntegrationTests.Infrastructure;

public class CacheServiceTests : IAsyncLifetime
{
    private IConnectionMultiplexer? _redis;
    private ICacheService? _cacheService;
    private ILogger<CacheService>? _logger;

    public async Task InitializeAsync()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<CacheService>();
        _cacheService = new CacheService(_redis, _logger);
        
        // Clear all keys before test
        var server = _redis!.GetServer(_redis.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.DisposeAsync();
        }
    }

    [Fact]
    public async Task SetAsync_WithValidData_StoresInCache()
    {
        // Arrange
        var key = "test_key";
        var value = new { Name = "Test", Value = 123 };

        // Act
        await _cacheService!.SetAsync(key, value);
        var result = await _cacheService.GetAsync<dynamic>(key);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_WithValidKey_ReturnsValue()
    {
        // Arrange
        var key = "test_key";
        var value = new TestData { Id = 1, Name = "Test" };
        await _cacheService!.SetAsync(key, value);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Id, result.Id);
        Assert.Equal(value.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WithInvalidKey_ReturnsNull()
    {
        // Act
        var result = await _cacheService!.GetAsync<TestData>("nonexistent_key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_SetsExpiry()
    {
        // Arrange
        var key = "expiring_key";
        var value = new TestData { Id = 1, Name = "Expiring" };
        var expiration = TimeSpan.FromSeconds(2);

        // Act
        await _cacheService!.SetAsync(key, value, expiration);
        var resultBefore = await _cacheService.GetAsync<TestData>(key);
        
        await Task.Delay(2500); // Wait for expiration
        var resultAfter = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.NotNull(resultBefore);
        Assert.Null(resultAfter);
    }

    [Fact]
    public async Task RemoveAsync_WithValidKey_DeletesFromCache()
    {
        // Arrange
        var key = "remove_key";
        var value = new TestData { Id = 1, Name = "ToRemove" };
        await _cacheService!.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var key = "exists_key";
        var value = new TestData { Id = 1, Name = "Exists" };
        await _cacheService!.SetAsync(key, value);

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidKey_ReturnsFalse()
    {
        // Act
        var exists = await _cacheService!.ExistsAsync("nonexistent_key");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task MultipleOperations_WorkCorrectly()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var values = new[]
        {
            new TestData { Id = 1, Name = "First" },
            new TestData { Id = 2, Name = "Second" },
            new TestData { Id = 3, Name = "Third" }
        };

        // Act
        for (int i = 0; i < keys.Length; i++)
        {
            await _cacheService!.SetAsync(keys[i], values[i]);
        }

        var results = new List<TestData>();
        foreach (var key in keys)
        {
            var result = await _cacheService.GetAsync<TestData>(key);
            if (result != null)
                results.Add(result);
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.NotNull(r.Name));
    }

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
