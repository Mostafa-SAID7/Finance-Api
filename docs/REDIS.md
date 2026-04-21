# Redis Caching Implementation Guide

## Overview

Redis is integrated into the FinanceControl API as a distributed caching layer to improve performance and reduce database load. This guide explains how to use Redis caching in your application.

## Architecture

### Components

- **Redis Server**: In-memory data store running on port 6379
- **StackExchange.Redis**: .NET client library for Redis communication
- **ICacheService**: Abstraction layer for cache operations
- **CacheService**: Implementation of cache operations with error handling and logging

### Data Flow

```
Application Request
    ↓
ICacheService (Interface)
    ↓
CacheService (Implementation)
    ↓
StackExchange.Redis (Client)
    ↓
Redis Server (Port 6379)
```

## Configuration

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "Redis": "redis-server:6379"
  }
}
```

For Redis with authentication:

```json
{
  "ConnectionStrings": {
    "Redis": "redis-server:6379,password=your-password"
  }
}
```

## Usage

### Dependency Injection

Redis is automatically registered in `Program.cs`:

```csharp
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddScoped<ICacheService, CacheService>();
```

### Basic Operations

#### Inject ICacheService

```csharp
public class ExpensesController : ControllerBase
{
    private readonly ICacheService _cacheService;

    public ExpensesController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
}
```

#### Get from Cache

```csharp
// Get a cached value
var cachedExpense = await _cacheService.GetAsync<Expense>("expense_123");

if (cachedExpense != null)
{
    return Ok(cachedExpense);
}

// If not in cache, fetch from database
var expense = await _dbContext.Expenses.FindAsync(123);
if (expense != null)
{
    // Store in cache for 1 hour
    await _cacheService.SetAsync("expense_123", expense, TimeSpan.FromHours(1));
}

return Ok(expense);
```

#### Set Cache Value

```csharp
var expense = new Expense { Id = 123, Description = "Office Supplies", Amount = 50.00m };

// Cache for 1 hour
await _cacheService.SetAsync("expense_123", expense, TimeSpan.FromHours(1));

// Cache indefinitely (until manually removed or Redis restart)
await _cacheService.SetAsync("expense_123", expense);
```

#### Remove from Cache

```csharp
// Remove a specific key
await _cacheService.RemoveAsync("expense_123");
```

#### Check Cache Existence

```csharp
// Check if key exists
bool exists = await _cacheService.ExistsAsync("expense_123");

if (exists)
{
    var value = await _cacheService.GetAsync<Expense>("expense_123");
}
```

## Caching Patterns

### Cache-Aside Pattern

```csharp
public async Task<Expense> GetExpenseAsync(int id)
{
    var cacheKey = $"expense_{id}";
    
    // Try to get from cache
    var cached = await _cacheService.GetAsync<Expense>(cacheKey);
    if (cached != null)
    {
        _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
        return cached;
    }
    
    // Cache miss - fetch from database
    _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
    var expense = await _dbContext.Expenses.FindAsync(id);
    
    if (expense != null)
    {
        // Store in cache for 1 hour
        await _cacheService.SetAsync(cacheKey, expense, TimeSpan.FromHours(1));
    }
    
    return expense;
}
```

### Cache Invalidation on Update

```csharp
public async Task<Expense> UpdateExpenseAsync(int id, UpdateExpenseRequest request)
{
    var expense = await _dbContext.Expenses.FindAsync(id);
    if (expense == null)
        throw new NotFoundException($"Expense {id} not found");
    
    // Update the entity
    expense.Description = request.Description;
    expense.Amount = request.Amount;
    
    // Save to database
    await _dbContext.SaveChangesAsync();
    
    // Invalidate cache
    var cacheKey = $"expense_{id}";
    await _cacheService.RemoveAsync(cacheKey);
    
    return expense;
}
```

### Cache Invalidation on Delete

```csharp
public async Task DeleteExpenseAsync(int id)
{
    var expense = await _dbContext.Expenses.FindAsync(id);
    if (expense == null)
        throw new NotFoundException($"Expense {id} not found");
    
    // Delete from database
    _dbContext.Expenses.Remove(expense);
    await _dbContext.SaveChangesAsync();
    
    // Invalidate cache
    var cacheKey = $"expense_{id}";
    await _cacheService.RemoveAsync(cacheKey);
}
```

## Key Naming Conventions

Use descriptive, hierarchical key names:

```csharp
// Entity by ID
$"expense_{id}"
$"income_{id}"

// Collections
"expenses:all"
"incomes:by_type:salary"

// User-specific data
$"user_{userId}:expenses"
$"user_{userId}:balance"

// Temporary data
$"otp_{email}"
$"session_{sessionId}"
```

## TTL (Time To Live) Guidelines

| Data Type | TTL | Reason |
|-----------|-----|--------|
| User Profile | 1 hour | Moderate change frequency |
| Expense/Income | 30 minutes | Frequently updated |
| Balance | 5 minutes | Real-time accuracy needed |
| List/Collection | 15 minutes | May change frequently |
| Configuration | 24 hours | Rarely changes |
| Temporary Data (OTP) | 5 minutes | Security requirement |

## Monitoring and Debugging

### Check Redis Connection

```csharp
public async Task<IActionResult> HealthCheck()
{
    try
    {
        var redis = HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var server = redis.GetServer(redis.GetEndPoints().First());
        var info = await server.InfoAsync();
        
        return Ok(new { status = "healthy", redis = "connected" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Redis health check failed");
        return StatusCode(503, new { status = "unhealthy", error = ex.Message });
    }
}
```

### View Cache Statistics

```bash
# Connect to Redis CLI
redis-cli

# Get all keys
KEYS *

# Get specific key
GET expense_123

# Get key info
INFO keyspace

# Monitor real-time commands
MONITOR

# Clear all cache
FLUSHALL

# Clear specific database
FLUSHDB
```

### Logging

Cache operations are automatically logged:

```
[Debug] Cache hit for key: expense_123
[Debug] Cache miss for key: expense_456
[Debug] Cache set for key: expense_789 with expiration: 01:00:00
[Debug] Cache removed for key: expense_123
[Error] Error retrieving cache for key: expense_999
```

## Docker Compose

Redis is configured in `docker-compose.yml`:

```yaml
redis:
  image: redis:7-alpine
  container_name: redis_container
  restart: always
  ports:
    - "6379:6379"
  command: redis-server --appendonly yes
  volumes:
    - redis_data:/data
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 10s
    timeout: 5s
    retries: 5
```

### Start Redis

```bash
docker-compose up -d redis
```

### Stop Redis

```bash
docker-compose down redis
```

### View Redis Logs

```bash
docker-compose logs -f redis
```

## Testing

### Unit Tests

```csharp
[Fact]
public async Task GetAsync_WithValidKey_ReturnsValue()
{
    // Arrange
    var key = "test_key";
    var value = new Expense { Id = 1, Description = "Test" };
    await _cacheService.SetAsync(key, value);

    // Act
    var result = await _cacheService.GetAsync<Expense>(key);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(value.Id, result.Id);
}
```

### Integration Tests

See `FinanceControl.IntegrationTests/Infrastructure/CacheServiceTests.cs` for comprehensive test cases.

Run tests:

```bash
dotnet test
```

## Performance Considerations

### Advantages

- **Reduced Database Load**: Frequently accessed data is served from memory
- **Faster Response Times**: In-memory access is much faster than database queries
- **Scalability**: Distributed cache supports horizontal scaling
- **Reduced Latency**: Network latency is minimal for local Redis

### Disadvantages

- **Memory Usage**: Cached data consumes Redis memory
- **Consistency**: Stale data may be served if cache isn't invalidated
- **Complexity**: Additional layer to manage and monitor

### Best Practices

1. **Cache Strategically**: Only cache frequently accessed, expensive-to-compute data
2. **Set Appropriate TTLs**: Balance between freshness and performance
3. **Invalidate Properly**: Remove cache when data changes
4. **Monitor Memory**: Watch Redis memory usage and eviction policies
5. **Handle Failures**: Gracefully handle Redis connection failures
6. **Use Meaningful Keys**: Make debugging easier with descriptive key names

## Troubleshooting

### Redis Connection Failed

```
Error: Unable to connect to Redis at localhost:6379
```

**Solution:**
- Verify Redis is running: `docker-compose ps`
- Check connection string in appsettings
- Verify firewall allows port 6379

### Out of Memory

```
Error: OOM command not allowed when used memory > 'maxmemory'
```

**Solution:**
- Increase Redis memory limit in docker-compose
- Implement cache eviction policy
- Review and optimize TTLs

### Stale Data

**Solution:**
- Implement proper cache invalidation
- Use shorter TTLs for frequently changing data
- Consider using event-driven invalidation

## References

- [StackExchange.Redis Documentation](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Documentation](https://redis.io/documentation)
- [Redis Best Practices](https://redis.io/topics/best-practices)
- [Caching Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
