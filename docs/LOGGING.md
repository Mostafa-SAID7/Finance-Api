# Serilog and Elasticsearch Logging Guide

## Overview

The FinanceControl API uses Serilog for structured logging with Elasticsearch as the log aggregation backend and Kibana for visualization. This guide explains how to use and monitor logs.

## Architecture

### Components

- **Serilog**: Structured logging framework for .NET
- **Elasticsearch**: Log aggregation and search engine (port 9200)
- **Kibana**: Log visualization and analysis tool (port 5601)
- **Console Sink**: Real-time log output to console (development)

### Data Flow

```
Application
    ↓
Serilog Logger
    ↓
├─ Console Sink (Development)
└─ Elasticsearch Sink (All environments)
    ↓
Elasticsearch (Port 9200)
    ↓
Kibana (Port 5601)
```

## Configuration

### Program.cs Setup

```csharp
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
            new Uri(context.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "finance-api-{0:yyyy.MM.dd}",
            MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
        });
});
```

### appsettings.Development.json

```json
{
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

### appsettings.Production.json

```json
{
  "Elasticsearch": {
    "Uri": "https://elasticsearch-cluster:9200"
  },
  "Serilog": {
    "MinimumLevel": "Warning"
  }
}
```

## Log Levels

| Level | Usage | Example |
|-------|-------|---------|
| Verbose | Detailed diagnostic information | Variable values, method entry/exit |
| Debug | Debugging information | Cache hits/misses, query execution |
| Information | General informational messages | Request received, operation completed |
| Warning | Warning messages | Deprecated API usage, performance issues |
| Error | Error messages | Exception occurred, operation failed |
| Fatal | Critical errors | Application crash, unrecoverable error |

## Logging in Controllers

### Basic Logging

```csharp
public class ExpensesController : ControllerBase
{
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ILogger<ExpensesController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpense(int id)
    {
        _logger.LogInformation("Fetching expense with ID: {ExpenseId}", id);
        
        try
        {
            var expense = await _dbContext.Expenses.FindAsync(id);
            if (expense == null)
            {
                _logger.LogWarning("Expense not found: {ExpenseId}", id);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully retrieved expense: {ExpenseId}", id);
            return Ok(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense: {ExpenseId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### Structured Logging

```csharp
// Log with structured properties
_logger.LogInformation(
    "Expense created: {ExpenseId}, Amount: {Amount}, Type: {ExpenseType}",
    expense.Id,
    expense.Amount,
    expense.Type
);

// Log with context
using (_logger.BeginScope(new Dictionary<string, object>
{
    { "UserId", userId },
    { "RequestId", requestId }
}))
{
    _logger.LogInformation("Processing user request");
}
```

### Performance Logging

```csharp
var stopwatch = Stopwatch.StartNew();

try
{
    var expenses = await _dbContext.Expenses.ToListAsync();
    stopwatch.Stop();
    
    _logger.LogInformation(
        "Query completed in {ElapsedMilliseconds}ms, returned {Count} records",
        stopwatch.ElapsedMilliseconds,
        expenses.Count
    );
}
catch (Exception ex)
{
    stopwatch.Stop();
    _logger.LogError(
        ex,
        "Query failed after {ElapsedMilliseconds}ms",
        stopwatch.ElapsedMilliseconds
    );
}
```

## Enrichment

Serilog automatically enriches logs with:

- **Environment Name**: Development, Staging, Production
- **Thread ID**: Which thread processed the request
- **Timestamp**: When the log was created
- **Level**: Log level (Information, Warning, Error, etc.)
- **Message Template**: The log message pattern
- **Properties**: Structured properties from the log call

### Custom Enrichment

```csharp
loggerConfig
    .Enrich.WithProperty("Application", "FinanceControl")
    .Enrich.WithProperty("Version", "2.0.0")
    .Enrich.When(le => le.Level == LogEventLevel.Error, e => e.WithProperty("AlertLevel", "High"));
```

## Elasticsearch Index Management

### Index Format

Logs are stored in daily indices:

```
finance-api-2026.04.21
finance-api-2026.04.22
finance-api-2026.04.23
```

### Index Lifecycle Management

```bash
# View all indices
curl http://localhost:9200/_cat/indices

# Delete old indices (older than 30 days)
curl -X DELETE http://localhost:9200/finance-api-2026.03.*

# Get index statistics
curl http://localhost:9200/finance-api-2026.04.21/_stats
```

## Kibana Usage

### Access Kibana

Open browser and navigate to: `http://localhost:5601`

### Create Index Pattern

1. Go to **Stack Management** → **Index Patterns**
2. Click **Create index pattern**
3. Enter pattern: `finance-api-*`
4. Select **@timestamp** as time field
5. Click **Create index pattern**

### Search Logs

1. Go to **Discover**
2. Select `finance-api-*` index pattern
3. Use KQL (Kibana Query Language) to search:

```
# Find all errors
level: "Error"

# Find errors in specific controller
logger: "FinanceControl.Api.Controllers.ExpensesController"

# Find logs with specific property
ExpenseId: 123

# Find logs in time range
@timestamp: [now-1h TO now]

# Combine conditions
level: "Error" AND logger: "FinanceControl.Api.Controllers*"
```

### Create Dashboards

1. Go to **Dashboards**
2. Click **Create dashboard**
3. Add visualizations:
   - Log count over time
   - Error rate
   - Top error messages
   - Request latency

### Set Up Alerts

1. Go to **Stack Management** → **Alerting**
2. Create alert rule:
   - Condition: `level: "Error"`
   - Threshold: More than 10 errors in 5 minutes
   - Action: Send email notification

## Docker Compose

Elasticsearch and Kibana are configured in `docker-compose.yml`:

```yaml
elasticsearch:
  image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
  container_name: elasticsearch_container
  restart: always
  ports:
    - "9200:9200"
    - "9300:9300"
  environment:
    - discovery.type=single-node
    - xpack.security.enabled=false
    - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
  volumes:
    - elasticsearch_data:/usr/share/elasticsearch/data

kibana:
  image: docker.elastic.co/kibana/kibana:8.11.0
  container_name: kibana_container
  restart: always
  ports:
    - "5601:5601"
  environment:
    - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
  depends_on:
    - elasticsearch
```

### Start Services

```bash
docker-compose up -d elasticsearch kibana
```

### View Logs

```bash
docker-compose logs -f elasticsearch
docker-compose logs -f kibana
```

## Log Analysis Examples

### Find Slow Queries

```
level: "Information" AND message: "Query completed" AND ElapsedMilliseconds: [1000 TO *]
```

### Track User Activity

```
UserId: 123 | stats count() by level
```

### Monitor Cache Performance

```
message: "Cache*" | stats count() by message
```

### Error Rate by Endpoint

```
level: "Error" | stats count() by logger
```

## Performance Considerations

### Log Volume

- **Development**: All logs (Information level)
- **Production**: Warning and Error only
- **Staging**: Information level

### Elasticsearch Tuning

```yaml
environment:
  - "ES_JAVA_OPTS=-Xms1g -Xmx1g"  # Increase heap for production
  - "indices.memory.index_buffer_size=30%"
```

### Index Retention

```bash
# Delete indices older than 30 days
curl -X DELETE http://localhost:9200/finance-api-*?q=@timestamp:[* TO now-30d]
```

## Troubleshooting

### Logs Not Appearing in Elasticsearch

1. Check Elasticsearch is running: `docker-compose ps`
2. Verify connection string in appsettings
3. Check Elasticsearch logs: `docker-compose logs elasticsearch`
4. Verify index pattern in Kibana

### High Disk Usage

1. Reduce log level in production
2. Implement index retention policy
3. Delete old indices
4. Increase Elasticsearch storage

### Slow Kibana Queries

1. Increase Kibana memory: `KIBANA_JAVA_OPTS=-Xms1g -Xmx1g`
2. Optimize index settings
3. Use time range filters
4. Archive old indices

## Best Practices

1. **Use Structured Logging**: Include relevant properties for better searchability
2. **Appropriate Log Levels**: Use correct level for each message
3. **Avoid Logging Sensitive Data**: Never log passwords, tokens, or PII
4. **Include Context**: Add request ID, user ID, or correlation ID
5. **Monitor Log Volume**: Watch for excessive logging
6. **Set Up Alerts**: Alert on critical errors
7. **Regular Cleanup**: Archive or delete old logs
8. **Use Correlation IDs**: Track requests across services

## References

- [Serilog Documentation](https://serilog.net/)
- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana Documentation](https://www.elastic.co/guide/en/kibana/current/index.html)
- [Structured Logging Best Practices](https://github.com/serilog/serilog/wiki)
