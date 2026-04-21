# Observability Stack Documentation

## Overview

The FinanceControl API includes a comprehensive observability stack consisting of:

- **Structured Logging**: Serilog with Elasticsearch and Kibana
- **Distributed Caching**: Redis for performance monitoring
- **Health Checks**: Built-in health check endpoints
- **Metrics**: Application performance metrics

This guide explains how to monitor and observe the entire system.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    FinanceControl API                        │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Controllers → Services → Database                   │   │
│  │       ↓           ↓          ↓                        │   │
│  │  Serilog Logger (Structured Logging)                 │   │
│  │       ↓                                               │   │
│  │  ├─ Console (Development)                            │   │
│  │  └─ Elasticsearch (All Environments)                 │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
         ↓                    ↓                    ↓
    ┌─────────┐         ┌──────────┐        ┌──────────┐
    │ Kibana  │         │ Redis    │        │ Database │
    │ (5601)  │         │ (6379)   │        │ (5432)   │
    └─────────┘         └──────────┘        └──────────┘
```

## Components

### 1. Serilog Logging

**Purpose**: Capture and aggregate application logs

**Configuration**:
- Minimum Level: Information (Development), Warning (Production)
- Sinks: Console + Elasticsearch
- Enrichment: Environment, Thread ID, Timestamp

**Access**: Kibana at `http://localhost:5601`

See [LOGGING.md](./LOGGING.md) for detailed guide.

### 2. Elasticsearch

**Purpose**: Store and index logs for fast searching

**Configuration**:
- Version: 8.11.0
- Port: 9200
- Single-node cluster
- Daily indices: `finance-api-{yyyy.MM.dd}`

**Access**: `http://localhost:9200`

### 3. Kibana

**Purpose**: Visualize and analyze logs

**Configuration**:
- Version: 8.11.0
- Port: 5601
- Connected to Elasticsearch

**Access**: `http://localhost:5601`

### 4. Redis

**Purpose**: Distributed caching for performance

**Configuration**:
- Version: 7-alpine
- Port: 6379
- Persistence: Enabled (AOF)

**Access**: `redis-cli` or `http://localhost:6379`

See [REDIS.md](./REDIS.md) for detailed guide.

### 5. PostgreSQL

**Purpose**: Primary data store

**Configuration**:
- Version: 15
- Port: 5432
- Database: FinanceControl

**Access**: `psql -h localhost -U postgres -d FinanceControl`

### 6. Kafka

**Purpose**: Event streaming and message broker

**Configuration**:
- Image: bashj79/kafka-kraft
- Port: 9092
- Single broker

**Access**: Kafka CLI tools

## Monitoring Workflows

### 1. Real-Time Log Monitoring

**Scenario**: Monitor application logs as they happen

**Steps**:
1. Open Kibana: `http://localhost:5601`
2. Go to **Discover**
3. Select `finance-api-*` index pattern
4. View logs in real-time
5. Use filters to narrow down:
   ```
   level: "Error"
   logger: "FinanceControl.Api.Controllers*"
   ```

### 2. Error Investigation

**Scenario**: Investigate an error that occurred

**Steps**:
1. Open Kibana
2. Go to **Discover**
3. Search for error:
   ```
   level: "Error" AND @timestamp: [now-1h TO now]
   ```
4. Click on log entry to view full details
5. Check related logs using correlation ID:
   ```
   RequestId: "abc-123-def"
   ```

### 3. Performance Analysis

**Scenario**: Identify slow operations

**Steps**:
1. Open Kibana
2. Search for slow queries:
   ```
   message: "Query completed" AND ElapsedMilliseconds: [1000 TO *]
   ```
3. Create visualization:
   - X-axis: Time
   - Y-axis: Average ElapsedMilliseconds
   - Group by: Logger

### 4. Cache Performance

**Scenario**: Monitor cache hit/miss ratio

**Steps**:
1. Open Kibana
2. Search for cache operations:
   ```
   message: "Cache*"
   ```
3. Create visualization:
   - Count by message (hit vs miss)
   - Trend over time

### 5. System Health Check

**Scenario**: Verify all services are running

**Steps**:
1. Check PostgreSQL:
   ```bash
   docker-compose ps postgres
   ```
2. Check Redis:
   ```bash
   redis-cli ping
   ```
3. Check Elasticsearch:
   ```bash
   curl http://localhost:9200/_cluster/health
   ```
4. Check Kibana:
   ```bash
   curl http://localhost:5601/api/status
   ```

## Key Metrics to Monitor

### Application Metrics

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| Error Rate | < 1% | > 5% |
| Response Time | < 200ms | > 1000ms |
| Cache Hit Rate | > 80% | < 50% |
| Database Query Time | < 100ms | > 500ms |
| Memory Usage | < 500MB | > 1GB |

### Infrastructure Metrics

| Component | Metric | Target | Alert |
|-----------|--------|--------|-------|
| PostgreSQL | Connections | < 20 | > 50 |
| PostgreSQL | Disk Usage | < 80% | > 90% |
| Redis | Memory | < 100MB | > 500MB |
| Elasticsearch | Disk Usage | < 80% | > 90% |
| Elasticsearch | Heap Usage | < 80% | > 90% |

## Dashboards

### Create Application Health Dashboard

1. Go to Kibana → **Dashboards** → **Create dashboard**
2. Add panels:

**Panel 1: Error Count**
```
Visualization: Bar chart
Query: level: "Error"
Time range: Last 24 hours
```

**Panel 2: Response Time Trend**
```
Visualization: Line chart
Query: message: "Query completed"
Metric: Average ElapsedMilliseconds
Time range: Last 24 hours
```

**Panel 3: Cache Performance**
```
Visualization: Pie chart
Query: message: "Cache*"
Group by: message
```

**Panel 4: Top Errors**
```
Visualization: Table
Query: level: "Error"
Group by: message
Sort by: count (descending)
```

## Alerts

### Set Up Error Alert

1. Go to **Stack Management** → **Alerting** → **Create rule**
2. Configure:
   - **Condition**: `level: "Error"`
   - **Threshold**: More than 10 errors in 5 minutes
   - **Action**: Send email to ops@company.com
   - **Frequency**: Check every 1 minute

### Set Up Performance Alert

1. Go to **Stack Management** → **Alerting** → **Create rule**
2. Configure:
   - **Condition**: `ElapsedMilliseconds: [1000 TO *]`
   - **Threshold**: More than 5 slow queries in 10 minutes
   - **Action**: Send Slack notification
   - **Frequency**: Check every 5 minutes

## Docker Compose Commands

### Start All Services

```bash
docker-compose up -d
```

### Start Specific Service

```bash
docker-compose up -d elasticsearch
docker-compose up -d kibana
docker-compose up -d redis
```

### View Logs

```bash
docker-compose logs -f elasticsearch
docker-compose logs -f kibana
docker-compose logs -f redis
```

### Check Service Status

```bash
docker-compose ps
```

### Stop Services

```bash
docker-compose down
```

### Remove Volumes (Clean Start)

```bash
docker-compose down -v
```

## Health Check Endpoints

### Application Health

```bash
curl http://localhost:5000/health
```

Response:
```json
{
  "status": "healthy",
  "checks": {
    "database": "healthy",
    "redis": "healthy",
    "elasticsearch": "healthy"
  }
}
```

### Elasticsearch Health

```bash
curl http://localhost:9200/_cluster/health
```

Response:
```json
{
  "cluster_name": "docker-cluster",
  "status": "green",
  "timed_out": false,
  "number_of_nodes": 1,
  "number_of_data_nodes": 1,
  "active_primary_shards": 0,
  "active_shards": 0,
  "relocating_shards": 0,
  "initializing_shards": 0,
  "unassigned_shards": 0,
  "delayed_unassigned_shards": 0,
  "number_of_pending_tasks": 0,
  "number_of_in_flight_fetch": 0,
  "task_max_waiting_in_queue_millis": 0,
  "active_shards_percent_as_number": 100.0
}
```

### Redis Health

```bash
redis-cli ping
```

Response:
```
PONG
```

## Troubleshooting

### Logs Not Appearing

1. Check Elasticsearch is running:
   ```bash
   docker-compose ps elasticsearch
   ```
2. Verify connection in appsettings
3. Check Elasticsearch logs:
   ```bash
   docker-compose logs elasticsearch
   ```

### Kibana Not Accessible

1. Check Kibana is running:
   ```bash
   docker-compose ps kibana
   ```
2. Wait 30 seconds for Kibana to initialize
3. Check Kibana logs:
   ```bash
   docker-compose logs kibana
   ```

### High Memory Usage

1. Check Elasticsearch memory:
   ```bash
   curl http://localhost:9200/_nodes/stats/jvm
   ```
2. Increase heap size in docker-compose:
   ```yaml
   environment:
     - "ES_JAVA_OPTS=-Xms1g -Xmx1g"
   ```

### Slow Queries

1. Check database performance:
   ```bash
   docker-compose logs postgres
   ```
2. Review slow query logs in Kibana
3. Add database indexes if needed

## Best Practices

1. **Monitor Proactively**: Set up alerts before issues occur
2. **Correlate Events**: Use request IDs to track requests across services
3. **Regular Reviews**: Review logs and metrics weekly
4. **Archive Old Logs**: Delete logs older than 30 days
5. **Document Runbooks**: Create runbooks for common issues
6. **Test Alerts**: Verify alerts work before production
7. **Capacity Planning**: Monitor trends to plan for growth
8. **Security**: Restrict access to sensitive logs

## Integration with CI/CD

### GitHub Actions

Add to `.github/workflows/deploy.yml`:

```yaml
- name: Check Application Health
  run: |
    curl -f http://localhost:5000/health || exit 1
    
- name: Verify Elasticsearch
  run: |
    curl -f http://localhost:9200/_cluster/health || exit 1
```

## References

- [Serilog Documentation](https://serilog.net/)
- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/)
- [Kibana Documentation](https://www.elastic.co/guide/en/kibana/current/)
- [Redis Documentation](https://redis.io/documentation)
- [Observability Best Practices](https://www.elastic.co/guide/en/observability/current/)
