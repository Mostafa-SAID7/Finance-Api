# Deployment Guide

## Docker Deployment

### Build Docker Image

```bash
docker build -f src/FinanceControl.Api/Dockerfile -t finance-api:latest .
```

### Run Docker Container

```bash
docker run -d \
  --name finance-api \
  -p 5000:80 \
  -p 5001:443 \
  -e ConnectionStrings__Postgres="Host=postgres:5432;Database=FinanceControl;Username=postgres;Password=admin" \
  -e Kafka__BootstrapServers="kafka:9092" \
  finance-api:latest
```

### Docker Compose

```bash
docker-compose up -d
```

## CI/CD Pipeline

The project includes GitHub Actions workflows for automated testing and deployment.

### Docker Image CI Workflow

**Trigger**: Push to `master` branch or Pull Request

**Steps**:
1. Checkout code
2. Setup Docker Buildx
3. Login to GitHub Container Registry (GHCR)
4. Extract metadata and generate tags
5. Build and push Docker image

**Image Tags**:
- `branch-name` - Branch-based tag
- `latest` - Latest on master branch
- `sha-<commit-hash>` - Commit-based tag
- Semantic versioning tags (if using git tags)

**Registry**: `ghcr.io/mostafa-said7/finance-api`

### Accessing Images

```bash
# Login to GHCR
echo ${{ secrets.GITHUB_TOKEN }} | docker login ghcr.io -u ${{ github.actor }} --password-stdin

# Pull image
docker pull ghcr.io/mostafa-said7/finance-api:latest

# Run image
docker run -d ghcr.io/mostafa-said7/finance-api:latest
```

## Environment Configuration

### Production Environment Variables

```bash
# Database
ConnectionStrings__Postgres=Host=prod-postgres:5432;Database=FinanceControl;Username=produser;Password=strongpassword

# Kafka
Kafka__BootstrapServers=prod-kafka:9092

# Logging
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft.AspNetCore=Error

# ASPNETCORE
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
```

### Secrets Management

Store sensitive data in:
- **GitHub Secrets** for CI/CD
- **Azure Key Vault** for production
- **Environment variables** for containers
- **.NET User Secrets** for local development

```bash
# Set user secret locally
dotnet user-secrets set "ConnectionStrings:Postgres" "your-connection-string"
```

## Database Deployment

### Initial Setup

1. Create PostgreSQL database
2. Run migrations (automatic on app startup)
3. Verify schema creation

### Backup & Recovery

```bash
# Backup database
docker exec postgres pg_dump -U postgres FinanceControl > backup.sql

# Restore database
docker exec -i postgres psql -U postgres FinanceControl < backup.sql
```

## Monitoring & Logging

### Application Insights

Add Application Insights for monitoring:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Logging

Configure structured logging:

```csharp
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
```

### Health Checks

Add health check endpoint:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinanceControlDbContext>()
    .AddKafka(new Uri("kafka:9092"));

app.MapHealthChecks("/health");
```

## Scaling

### Horizontal Scaling

- Deploy multiple instances behind a load balancer
- Use connection pooling for database
- Implement caching layer (Redis)
- Use Kafka for asynchronous processing

### Vertical Scaling

- Increase container resource limits
- Optimize database queries
- Implement query caching
- Use connection pooling

## Security Checklist

- [ ] Enable HTTPS/TLS
- [ ] Implement authentication (OAuth2/JWT)
- [ ] Add authorization policies
- [ ] Validate all inputs
- [ ] Use secrets manager for sensitive data
- [ ] Enable CORS only for trusted origins
- [ ] Implement rate limiting
- [ ] Add request logging and monitoring
- [ ] Regular security updates
- [ ] Database encryption at rest

## Rollback Procedure

### Docker Image Rollback

```bash
# Pull previous image version
docker pull ghcr.io/mostafa-said7/finance-api:previous-tag

# Stop current container
docker stop finance-api

# Run previous version
docker run -d --name finance-api ghcr.io/mostafa-said7/finance-api:previous-tag
```

### Database Rollback

```bash
# Revert to previous migration
dotnet ef migrations remove --project src/FinanceControl.Api

# Restore from backup
docker exec -i postgres psql -U postgres FinanceControl < backup.sql
```

## Troubleshooting

### Container Won't Start
- Check logs: `docker logs finance-api`
- Verify environment variables
- Check database connectivity
- Verify Kafka connectivity

### Database Connection Timeout
- Verify PostgreSQL is running
- Check connection string
- Verify network connectivity
- Check firewall rules

### High Memory Usage
- Monitor with `docker stats`
- Check for memory leaks
- Optimize queries
- Increase container limits

## Performance Tuning

- Enable response compression
- Implement caching strategies
- Optimize database indexes
- Use async operations
- Monitor and profile application
- Implement pagination for large datasets
