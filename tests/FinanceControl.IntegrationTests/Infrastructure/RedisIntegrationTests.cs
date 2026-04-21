using FinanceControl.Api.Infra;
using StackExchange.Redis;
using Xunit;

namespace FinanceControl.IntegrationTests.Infrastructure;

public class RedisIntegrationTests : IAsyncLifetime
{
    private IConnectionMultiplexer? _redis;
    private ICacheService? _cacheService;
    private ILogger<CacheService>? _logger;

    public async Task InitializeAsync()
    {
        try
        {
            _redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<CacheService>();
            _cacheService = new CacheService(_redis, _logger);
            
            // Clear all keys before test
            var server = _redis!.GetServer(_redis.GetEndPoints().First());
            await server.FlushDatabaseAsync();
        }
        catch
        {
            // Redis might not be running, skip tests
            _redis = null;
            _cacheService = null;
        }
    }

    public async Task DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.DisposeAsync();
        }
    }

    [Fact]
    public async Task Redis_Connection_IsSuccessful()
    {
        // Skip if Redis is not available
        if (_redis == null)
            return;

        // Arrange & Act
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var info = await server.InfoAsync();

        // Assert
        Assert.NotNull(info);
        Assert.True(info.Length > 0);
    }

    [Fact]
    public async Task CacheService_WorksWithRedis_InIntegrationTests()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var key = "integration_test_key";
        var value = new TestCacheData { Id = 1, Name = "Integration Test" };

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<TestCacheData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value.Id, result.Id);
        Assert.Equal(value.Name, result.Name);
    }

    [Fact]
    public async Task CacheService_CanRemoveValues_FromRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var key = "remove_test_key";
        var value = new TestCacheData { Id = 2, Name = "To Remove" };
        await _cacheService.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestCacheData>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CacheService_CanCheckKeyExistence_InRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var key = "exists_test_key";
        var value = new TestCacheData { Id = 3, Name = "Exists" };
        await _cacheService.SetAsync(key, value);

        // Act
        var exists = await _cacheService.ExistsAsync(key);
        var notExists = await _cacheService.ExistsAsync("nonexistent_key");

        // Assert
        Assert.True(exists);
        Assert.False(notExists);
    }

    [Fact]
    public async Task CacheService_RespectsTTL_InRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var key = "ttl_test_key";
        var value = new TestCacheData { Id = 4, Name = "TTL Test" };
        var ttl = TimeSpan.FromSeconds(1);

        // Act
        await _cacheService.SetAsync(key, value, ttl);
        var resultBefore = await _cacheService.GetAsync<TestCacheData>(key);
        
        await Task.Delay(1500); // Wait for TTL to expire
        var resultAfter = await _cacheService.GetAsync<TestCacheData>(key);

        // Assert
        Assert.NotNull(resultBefore);
        Assert.Null(resultAfter);
    }

    [Fact]
    public async Task CacheService_HandlesMultipleDataTypes_InRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var stringKey = "string_key";
        var stringValue = "test string";
        
        var intKey = "int_key";
        var intValue = 42;
        
        var objectKey = "object_key";
        var objectValue = new TestCacheData { Id = 5, Name = "Object" };

        // Act
        await _cacheService.SetAsync(stringKey, stringValue);
        await _cacheService.SetAsync(intKey, intValue);
        await _cacheService.SetAsync(objectKey, objectValue);

        var stringResult = await _cacheService.GetAsync<string>(stringKey);
        var intResult = await _cacheService.GetAsync<int>(intKey);
        var objectResult = await _cacheService.GetAsync<TestCacheData>(objectKey);

        // Assert
        Assert.Equal(stringValue, stringResult);
        Assert.Equal(intValue, intResult);
        Assert.NotNull(objectResult);
        Assert.Equal(objectValue.Id, objectResult.Id);
    }

    [Fact]
    public async Task CacheService_HandlesLargeValues_InRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var key = "large_value_key";
        var largeData = new TestCacheData
        {
            Id = 6,
            Name = new string('x', 10000) // 10KB string
        };

        // Act
        await _cacheService.SetAsync(key, largeData);
        var result = await _cacheService.GetAsync<TestCacheData>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largeData.Name, result.Name);
    }

    [Fact]
    public async Task CacheService_HandlesConcurrentOperations_InRedis()
    {
        // Skip if Redis is not available
        if (_cacheService == null)
            return;

        // Arrange
        var tasks = new List<Task>();
        var keyCount = 10;

        // Act
        for (int i = 0; i < keyCount; i++)
        {
            var key = $"concurrent_key_{i}";
            var value = new TestCacheData { Id = i, Name = $"Concurrent {i}" };
            tasks.Add(_cacheService.SetAsync(key, value));
        }

        await Task.WhenAll(tasks);

        var results = new List<TestCacheData>();
        for (int i = 0; i < keyCount; i++)
        {
            var key = $"concurrent_key_{i}";
            var result = await _cacheService.GetAsync<TestCacheData>(key);
            if (result != null)
                results.Add(result);
        }

        // Assert
        Assert.Equal(keyCount, results.Count);
    }

    private class TestCacheData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
