# Troubleshooting Guide

Solutions for common issues.

## Application Won't Start

### Error: "It was not possible to connect to the redis server"

**Cause**: Redis container not running or not accessible

**Solution**:
```bash
# Check if Redis is running
docker ps | grep redis

# If not running, start it
docker-compose up -d redis

# If still failing, restart
docker-compose restart redis

# Check Redis logs
docker-compose logs redis
```

**Note**: Redis is optional. App will work without it with graceful fallback.

### Error: "Failed to connect to Kafka"

**Cause**: Kafka not fully started yet

**Solution**:
```bash
# Wait 30 seconds for Kafka to start
sleep 30

# Then run app
dotnet run

# Or check Kafka logs
docker-compose logs kafka
```

### Error: "Database connection failed"

**Cause**: PostgreSQL not running or migrations failed

**Solution**:
```bash
# Check PostgreSQL
docker ps | grep postgres

# View logs
docker-compose logs postgres

# Reset database
docker-compose down -v
docker-compose up -d
dotnet run
```

### Error: "Port 5012 already in use"

**Cause**: Another process using the port

**Solution**:
```bash
# Find process using port
lsof -i :5012

# Kill it
kill -9 <PID>

# Or use different port
dotnet run --urls "http://localhost:5013"
```

## Docker Issues

### Containers Won't Start

**Symptom**: `docker-compose up` hangs or fails

**Solution**:
```bash
# Check Docker daemon
docker ps

# View detailed logs
docker-compose logs

# Restart Docker
docker-compose restart

# Or rebuild
docker-compose down
docker-compose up -d --build
```

### Out of Memory

**Symptom**: "Cannot allocate memory" or containers killed

**Solution**:
```bash
# Increase Docker memory
# Docker Desktop → Settings → Resources → Memory: 4GB+

# Or reduce containers
docker-compose down
docker-compose up -d postgres kafka
# Skip Redis/Elasticsearch if not needed
```

### Disk Space Full

**Symptom**: "No space left on device"

**Solution**:
```bash
# Clean up Docker
docker system prune -a

# Remove old images
docker image prune -a

# Remove volumes
docker volume prune

# Check disk space
df -h
```

### Port Conflicts

**Symptom**: "Address already in use"

**Solution**:
```bash
# Find what's using the port
lsof -i :5432  # PostgreSQL
lsof -i :9092  # Kafka
lsof -i :6379  # Redis
lsof -i :9200  # Elasticsearch

# Kill process
kill -9 <PID>

# Or change port in docker-compose.yml
# Change "5432:5432" to "5433:5432"
```

## Database Issues

### Migrations Not Applied

**Symptom**: Tables don't exist, queries fail

**Solution**:
```bash
# Check migration logs
# Look for "Migration finished" in app output

# If missing, reset database
docker-compose down -v
docker-compose up -d postgres
dotnet run

# Verify tables exist
docker exec postgres_container psql -U postgres -d FinanceControl -c "\dt"
```

### Connection String Wrong

**Symptom**: "Failed to connect to database"

**Solution**:
```bash
# Check appsettings.Development.json
cat src/FinanceControl.Api/appsettings.Development.json

# Verify connection string
# Should be: Host=localhost:5432;Database=FinanceControl;Username=postgres;Password=admin

# Test connection
docker exec postgres_container psql -U postgres -d FinanceControl -c "SELECT 1"
```

### Data Corruption

**Symptom**: Queries return wrong data or errors

**Solution**:
```bash
# Backup data (if needed)
docker exec postgres_container pg_dump -U postgres FinanceControl > backup.sql

# Reset database
docker-compose down -v
docker-compose up -d postgres
dotnet run

# Restore from backup (if needed)
docker exec -i postgres_container psql -U postgres FinanceControl < backup.sql
```

## Redis Issues

### Redis Connection Timeout

**Symptom**: "Connection timeout" or "Unable to connect"

**Solution**:
```bash
# Check if Redis is running
docker ps | grep redis

# If not, start it
docker-compose up -d redis

# Wait for it to be ready
docker-compose logs redis | grep "Ready to accept"

# Test connection
docker exec redis_container redis-cli ping
# Should return: PONG
```

### Redis Memory Full

**Symptom**: "OOM command not allowed when used memory > maxmemory"

**Solution**:
```bash
# Check Redis memory
docker exec redis_container redis-cli info memory

# Clear cache
docker exec redis_container redis-cli FLUSHALL

# Or increase memory in docker-compose.yml
# Add: --maxmemory 512mb
```

### Redis Persistence Issues

**Symptom**: Data lost after restart

**Solution**:
```bash
# Check if persistence is enabled
docker exec redis_container redis-cli CONFIG GET save

# Enable persistence in docker-compose.yml
# Add: --appendonly yes

# Restart Redis
docker-compose restart redis
```

## Kafka Issues

### Kafka Connection Failed

**Symptom**: "Failed to connect to Kafka broker"

**Solution**:
```bash
# Check if Kafka is running
docker ps | grep kafka

# Wait for Kafka to start (takes ~30 seconds)
sleep 30

# Check Kafka logs
docker-compose logs kafka

# Test connection
docker exec kafka_container kafka-broker-api-versions.sh --bootstrap-server localhost:9092
```

### Topics Not Created

**Symptom**: "Topic does not exist"

**Solution**:
```bash
# List topics
docker exec kafka_container kafka-topics.sh --list --bootstrap-server localhost:9092

# Create topic manually
docker exec kafka_container kafka-topics.sh --create \
  --bootstrap-server localhost:9092 \
  --topic expenses \
  --partitions 1 \
  --replication-factor 1

# Or restart app (auto-creates topics)
dotnet run
```

### Messages Not Publishing

**Symptom**: Events not appearing in Kafka

**Solution**:
```bash
# Check Kafka logs
docker-compose logs kafka

# Verify broker is healthy
docker exec kafka_container kafka-broker-api-versions.sh --bootstrap-server localhost:9092

# Check if topic exists
docker exec kafka_container kafka-topics.sh --list --bootstrap-server localhost:9092

# Consume messages to verify
docker exec kafka_container kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic expenses \
  --from-beginning
```

## Elasticsearch Issues

### Elasticsearch Won't Start

**Symptom**: Container exits immediately

**Solution**:
```bash
# Check logs
docker-compose logs elasticsearch

# Common issue: vm.max_map_count too low
# Linux only:
sudo sysctl -w vm.max_map_count=262144

# Or disable Elasticsearch if not needed
# Comment out in docker-compose.yml
```

### Can't Connect to Elasticsearch

**Symptom**: "Connection refused" or "Connection timeout"

**Solution**:
```bash
# Check if running
docker ps | grep elasticsearch

# Wait for startup (takes ~30 seconds)
sleep 30

# Test connection
curl http://localhost:9200

# Check logs
docker-compose logs elasticsearch
```

### Kibana Can't Connect to Elasticsearch

**Symptom**: Kibana shows "Unable to connect"

**Solution**:
```bash
# Check Elasticsearch is running
docker ps | grep elasticsearch

# Verify Elasticsearch is healthy
curl http://localhost:9200/_cluster/health

# Restart Kibana
docker-compose restart kibana

# Check Kibana logs
docker-compose logs kibana
```

## Test Issues

### Tests Timeout

**Symptom**: Tests hang or timeout after 60 seconds

**Solution**:
```bash
# Run individual test
dotnet test --filter "ClassName=CacheServiceTests"

# Increase timeout
dotnet test --logger "console;verbosity=detailed"

# Check Docker resources
docker stats
```

### Tests Fail with "Port Already in Use"

**Symptom**: "Address already in use" during test

**Solution**:
```bash
# Kill process using port
lsof -i :5432
kill -9 <PID>

# Or restart Docker
docker-compose restart

# Then run tests
dotnet test
```

### Tests Fail with "Cannot Connect to Docker"

**Symptom**: "Cannot connect to Docker daemon"

**Solution**:
```bash
# Start Docker
docker ps

# Or restart Docker daemon
# Windows: Restart Docker Desktop
# Mac: Restart Docker Desktop
# Linux: sudo systemctl restart docker

# Then run tests
dotnet test
```

## Performance Issues

### Application Slow

**Symptom**: Requests take >1 second

**Solution**:
```bash
# Check CPU/Memory
docker stats

# Check database performance
docker exec postgres_container psql -U postgres -d FinanceControl -c "SELECT * FROM pg_stat_statements LIMIT 10"

# Check Redis
docker exec redis_container redis-cli --stat

# Check Kafka lag
docker exec kafka_container kafka-consumer-groups.sh --bootstrap-server localhost:9092 --list
```

### High Memory Usage

**Symptom**: Docker containers using lots of memory

**Solution**:
```bash
# Check memory usage
docker stats

# Reduce cache size
docker exec redis_container redis-cli CONFIG SET maxmemory 256mb

# Or restart containers
docker-compose restart
```

### Slow Database Queries

**Symptom**: Database operations slow

**Solution**:
```bash
# Check slow queries
docker exec postgres_container psql -U postgres -d FinanceControl -c "SELECT * FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10"

# Add indexes if needed
docker exec postgres_container psql -U postgres -d FinanceControl -c "CREATE INDEX idx_expenses_date ON \"Expenses\"(\"Date\")"

# Analyze query plan
docker exec postgres_container psql -U postgres -d FinanceControl -c "EXPLAIN ANALYZE SELECT * FROM \"Expenses\""
```

## Logging Issues

### No Logs in Kibana

**Symptom**: Kibana shows no data

**Solution**:
```bash
# Check Elasticsearch has data
curl http://localhost:9200/_cat/indices

# Check index pattern
# Kibana → Stack Management → Index Patterns → Create "finance-api-*"

# Check app is logging
# Look for "WriteTo.Elasticsearch" in logs

# Restart Elasticsearch
docker-compose restart elasticsearch
```

### Logs Not Appearing in Console

**Symptom**: No output from application

**Solution**:
```bash
# Check log level in appsettings.Development.json
# Should be "Information" or lower

# Verify Serilog is configured
# Check Program.cs for UseSerilog()

# Run with verbose output
dotnet run --verbosity diagnostic
```

## Getting Help

### Check Logs

```bash
# Application logs
dotnet run

# Docker logs
docker-compose logs

# Specific service
docker-compose logs postgres
docker-compose logs kafka
docker-compose logs redis
docker-compose logs elasticsearch

# Follow logs
docker-compose logs -f
```

### Check Status

```bash
# Container status
docker-compose ps

# Docker stats
docker stats

# Network connectivity
docker network ls
docker network inspect finance-api_default
```

### Reset Everything

```bash
# Stop all containers
docker-compose down

# Remove volumes (WARNING: deletes data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Start fresh
docker-compose up -d
dotnet run
```

## Still Having Issues?

1. **Check logs**: `docker-compose logs`
2. **Check status**: `docker-compose ps`
3. **Check GitHub Issues**: https://github.com/Mostafa-SAID7/Finance-Api/issues
4. **Read documentation**: See `/docs` folder
5. **Reset everything**: `docker-compose down -v && docker-compose up -d`

---

**Last Updated**: April 21, 2026
**Status**: Comprehensive troubleshooting guide
