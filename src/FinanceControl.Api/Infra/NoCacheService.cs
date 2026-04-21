#nullable enable
namespace FinanceControl.Api.Infra;

/// <summary>
/// No-op cache service used when Redis is unavailable.
/// All operations are logged but no caching is performed.
/// </summary>
public class NoCacheService : ICacheService
{
    private readonly ILogger<NoCacheService> _logger;

    public NoCacheService(ILogger<NoCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _logger.LogDebug("Cache disabled - returning default for key: {Key}", key);
        return Task.FromResult(default(T?));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _logger.LogDebug("Cache disabled - skipping set for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _logger.LogDebug("Cache disabled - skipping remove for key: {Key}", key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        _logger.LogDebug("Cache disabled - returning false for key: {Key}", key);
        return Task.FromResult(false);
    }
}
