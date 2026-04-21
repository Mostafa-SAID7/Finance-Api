# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2026-04-21

### Added

#### Infrastructure & Observability
- **Redis Integration**
  - Added StackExchange.Redis 2.7.10 for distributed caching
  - Created `CacheService` interface and implementation for cache operations
  - Supports get, set, remove, and exists operations with TTL support
  - Automatic JSON serialization/deserialization
  - Comprehensive error logging and cache hit/miss tracking

- **Serilog Structured Logging**
  - Integrated Serilog 4.0.1 for structured logging
  - Added Serilog.AspNetCore 8.0.1 for ASP.NET Core integration
  - Configured console sink for development
  - Added environment and thread ID enrichment
  - Automatic request/response logging

- **Elasticsearch Integration**
  - Added Serilog.Sinks.Elasticsearch 9.0.3 for log aggregation
  - Configured automatic template registration
  - Index format: `finance-api-{yyyy.MM.dd}` for daily log rotation
  - Minimum log level: Information

- **Kibana Visualization**
  - Added Kibana 8.11.0 container for log visualization
  - Accessible at `http://localhost:5601`
  - Pre-configured to connect to Elasticsearch

#### Docker & Deployment
- **Enhanced docker-compose.yml**
  - Updated PostgreSQL to version 15 with persistent volumes
  - Added Redis 7-alpine with persistent storage
  - Added Elasticsearch 8.11.0 with single-node configuration
  - Added Kibana 8.11.0 for log visualization
  - Added health checks for all services
  - Configured proper environment variables and networking
  - Added volume management for data persistence

#### Configuration
- **Updated appsettings.Development.json**
  - Added Redis connection string configuration
  - Added Elasticsearch URI configuration
  - Maintained existing PostgreSQL and Kafka settings

#### Dependencies
- `StackExchange.Redis` 2.7.10 - Redis client library
- `Serilog` 4.0.1 - Structured logging framework
- `Serilog.AspNetCore` 8.0.1 - ASP.NET Core integration
- `Serilog.Sinks.Elasticsearch` 9.0.3 - Elasticsearch sink
- `Serilog.Enrichers.Environment` 2.3.0 - Environment enrichment
- `Serilog.Enrichers.Thread` 3.1.0 - Thread ID enrichment

### Changed

#### Program.cs
- Integrated Serilog as the logging provider
- Added Redis connection multiplexer registration
- Added ICacheService dependency injection
- Configured Elasticsearch sink with automatic template registration
- Added environment and thread enrichment to logs

#### Database
- Cleaned up duplicate migrations (removed Data/Migrations folder from project)
- Kept only Infra/Migrations as the active migration folder
- Migration: `20230720004620_InitialMigration` (Expenses and Incomes tables)

### Fixed

#### Migration Issues
- **Resolved duplicate migrations**: Removed inactive Data/Migrations folder
  - `Data/Migrations/20230720004358_InitialMigration.cs` (inactive - excluded from project)
  - `Infra/Migrations/20230720004620_InitialMigration.cs` (active - used by EF Core)
  - Both created identical schema; only Infra version is now used
  - Prevents migration conflicts and confusion

#### Configuration
- Updated PostgreSQL password from `P@ssw0rd` to `admin` for consistency
- Added proper health checks for all Docker services
- Configured proper networking between services

### Documentation

#### New Files
- `CHANGELOG.md` - This file, tracking all changes and versions
- `docs/REDIS.md` - Redis caching implementation guide with usage patterns, configuration, and best practices
- `docs/LOGGING.md` - Serilog and Elasticsearch logging guide with Kibana usage and log analysis
- `docs/OBSERVABILITY.md` - Complete observability stack documentation with monitoring workflows and dashboards

### Testing

#### New Test Cases
- **CacheService Tests** (`CacheServiceTests.cs`)
  - `GetAsync_WithValidKey_ReturnsValue` - Verify cache retrieval
  - `GetAsync_WithInvalidKey_ReturnsNull` - Verify cache miss handling
  - `SetAsync_WithValue_StoresInCache` - Verify cache storage
  - `SetAsync_WithExpiration_SetsExpiry` - Verify TTL functionality
  - `RemoveAsync_WithValidKey_DeletesFromCache` - Verify cache deletion
  - `ExistsAsync_WithValidKey_ReturnsTrue` - Verify key existence check
  - `ExistsAsync_WithInvalidKey_ReturnsFalse` - Verify non-existent key check
  - `MultipleOperations_WorkCorrectly` - Verify concurrent operations

- **Redis Integration Tests** (`RedisIntegrationTests.cs`)
  - `Redis_Connection_IsSuccessful` - Verify Redis connectivity
  - `CacheService_WorksWithRedis_InIntegrationTests` - Verify cache operations
  - `CacheService_CanRemoveValues_FromRedis` - Verify cache removal
  - `CacheService_CanCheckKeyExistence_InRedis` - Verify key existence check
  - `CacheService_RespectsTTL_InRedis` - Verify TTL expiration
  - `CacheService_HandlesMultipleDataTypes_InRedis` - Verify type handling
  - `CacheService_HandlesLargeValues_InRedis` - Verify large data handling
  - `CacheService_HandlesConcurrentOperations_InRedis` - Verify concurrent access

- **Logging Integration Tests** (`LoggingIntegrationTests.cs`)
  - `Application_StartsSuccessfully_WithSerilogConfigured` - Verify Serilog initialization
  - `DatabaseContext_IsAvailable_InApplication` - Verify database connectivity
  - `Logger_IsConfigured_InDependencyInjection` - Verify logger registration
  - `HealthCheck_ReturnsSuccessful_WhenServicesAreRunning` - Verify application health

### Infrastructure Changes

#### Docker Services
| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| PostgreSQL | postgres:15 | 5432 | Primary database |
| Kafka | bashj79/kafka-kraft | 9092 | Event streaming |
| Redis | redis:7-alpine | 6379 | Distributed cache |
| Elasticsearch | elasticsearch:8.11.0 | 9200 | Log aggregation |
| Kibana | kibana:8.11.0 | 5601 | Log visualization |

#### Health Checks
All services now include health checks:
- PostgreSQL: `pg_isready` command
- Kafka: Broker API versions check
- Redis: `redis-cli ping` command
- Elasticsearch: Cluster health endpoint
- Kibana: Status API endpoint

### Performance Improvements

- **Caching Layer**: Reduces database queries for frequently accessed data
- **Structured Logging**: Enables efficient log searching and analysis
- **Log Aggregation**: Centralized logging for better observability
- **Distributed Cache**: Supports horizontal scaling with shared cache

### Breaking Changes

None - This is a backward-compatible release with new features.

### Migration Guide

#### For Existing Deployments

1. **Update NuGet Packages**
   ```bash
   dotnet restore
   ```

2. **Update Configuration**
   - Add Redis connection string to appsettings
   - Add Elasticsearch URI to appsettings
   - Update PostgreSQL password if needed

3. **Update Docker Compose**
   ```bash
   docker-compose down
   docker-compose up -d
   ```

4. **Verify Services**
   - PostgreSQL: `psql -h localhost -U postgres -d FinanceControl`
   - Redis: `redis-cli ping`
   - Elasticsearch: `curl http://localhost:9200/_cluster/health`
   - Kibana: Visit `http://localhost:5601`

### Known Issues

None at this time.

### Future Roadmap

- [ ] Add distributed tracing with OpenTelemetry
- [ ] Implement circuit breaker pattern for external services
- [ ] Add metrics collection with Prometheus
- [ ] Implement API rate limiting
- [ ] Add authentication and authorization
- [ ] Implement event sourcing for audit trail
- [ ] Add GraphQL API support

### Contributors

- Mostafa SAID

---

## [1.0.0] - 2026-04-20

### Initial Release

- ASP.NET Core 9 API with PostgreSQL
- Kafka event streaming integration
- Integration tests with Testcontainers
- Docker support with multi-stage builds
- Swagger/OpenAPI documentation
- GitHub Actions CI/CD pipeline
