# Complete Setup Guide

Detailed setup instructions for all environments.

## System Requirements

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET SDK | 9.0+ | Runtime & Build |
| Docker | 20.10+ | Containers |
| Docker Compose | 2.0+ | Orchestration |
| Git | 2.30+ | Version control |
| RAM | 4GB+ | Containers + App |
| Disk | 5GB+ | Images + Data |

## Installation

### Windows

#### 1. Install .NET 9 SDK
```powershell
# Using Chocolatey
choco install dotnet-sdk-9.0

# Or download from https://dotnet.microsoft.com/download
```

#### 2. Install Docker Desktop
```powershell
# Using Chocolatey
choco install docker-desktop

# Or download from https://www.docker.com/products/docker-desktop
```

#### 3. Verify Installation
```powershell
dotnet --version
docker --version
docker-compose --version
```

### macOS

#### 1. Install .NET 9 SDK
```bash
# Using Homebrew
brew install dotnet-sdk-9.0

# Or download from https://dotnet.microsoft.com/download
```

#### 2. Install Docker Desktop
```bash
# Using Homebrew
brew install --cask docker

# Or download from https://www.docker.com/products/docker-desktop
```

#### 3. Verify Installation
```bash
dotnet --version
docker --version
docker-compose --version
```

### Linux (Ubuntu/Debian)

#### 1. Install .NET 9 SDK
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0

# Add to PATH
export PATH=$PATH:$HOME/.dotnet
```

#### 2. Install Docker
```bash
sudo apt-get update
sudo apt-get install docker.io docker-compose

# Add user to docker group
sudo usermod -aG docker $USER
```

#### 3. Verify Installation
```bash
dotnet --version
docker --version
docker-compose --version
```

## Project Setup

### 1. Clone Repository
```bash
git clone https://github.com/Mostafa-SAID7/Finance-Api.git
cd FinanceControl
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build Project
```bash
dotnet build
```

### 4. Start Infrastructure

Navigate to API directory:
```bash
cd src/FinanceControl.Api
```

Start all services:
```bash
docker-compose up -d
```

Verify services are running:
```bash
docker-compose ps
```

Expected output:
```
NAME                COMMAND                  SERVICE             STATUS
postgres_container  "docker-entrypoint.s…"   postgres            Up 2 minutes
kafka_container     "/bin/start_kafka.sh"    kafka               Up 2 minutes
redis_container     "redis-server"           redis               Up 2 minutes
elasticsearch       "/bin/tini -- /usr/l…"   elasticsearch       Up 2 minutes
kibana              "/bin/tini -- /usr/l…"   kibana              Up 2 minutes
```

### 5. Run Application

From project root:
```bash
dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
```

Or from API directory:
```bash
cd src/FinanceControl.Api
dotnet run
```

Expected output:
```
[HH:MM:SS INF] Migrating database...
[HH:MM:SS INF] Migration finished
[HH:MM:SS INF] Now listening on: https://localhost:7045
[HH:MM:SS INF] Now listening on: http://localhost:5012
[HH:MM:SS INF] Application started. Press Ctrl+C to shut down.
```

## Service Configuration

### PostgreSQL
- **Host**: localhost
- **Port**: 5432
- **Database**: FinanceControl
- **Username**: postgres
- **Password**: admin
- **Connection String**: `Host=localhost:5432;Database=FinanceControl;Username=postgres;Password=admin`

### Kafka
- **Host**: localhost
- **Port**: 9092
- **Bootstrap Servers**: localhost:9092
- **Topics**: expenses, incomes (auto-created)

### Redis
- **Host**: localhost
- **Port**: 6379
- **Connection String**: `localhost:6379`
- **Status**: Optional (app works without it)

### Elasticsearch
- **Host**: localhost
- **Port**: 9200
- **URL**: http://localhost:9200
- **Index Pattern**: finance-api-*

### Kibana
- **URL**: http://localhost:5601
- **Purpose**: View logs from Elasticsearch

## Configuration Files

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost:5432;Database=FinanceControl;Username=postgres;Password=admin",
    "Redis": "localhost:6379"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  }
}
```

### docker-compose.yml
Defines all services with:
- Health checks
- Volume persistence
- Network configuration
- Environment variables

## Verification Checklist

- [ ] .NET 9 SDK installed: `dotnet --version`
- [ ] Docker running: `docker ps`
- [ ] Repository cloned
- [ ] Dependencies restored: `dotnet restore`
- [ ] Project builds: `dotnet build`
- [ ] Containers started: `docker-compose ps`
- [ ] Application runs: `dotnet run`
- [ ] Swagger accessible: http://localhost:5012/swagger
- [ ] Database migrated: Check logs for "Migration finished"
- [ ] Can create expense: `curl -X POST http://localhost:5012/expenses ...`

## Troubleshooting

### Docker Issues

**Containers won't start**
```bash
# Check Docker daemon
docker ps

# View logs
docker-compose logs

# Restart Docker
docker-compose restart
```

**Port conflicts**
```bash
# Find process using port
lsof -i :5432  # PostgreSQL
lsof -i :9092  # Kafka
lsof -i :6379  # Redis
lsof -i :9200  # Elasticsearch

# Kill process
kill -9 <PID>
```

### Application Issues

**Database migration fails**
```bash
# Reset database
docker-compose down -v
docker-compose up -d
dotnet run
```

**Can't connect to services**
```bash
# Check container health
docker-compose ps

# View service logs
docker-compose logs postgres
docker-compose logs kafka
docker-compose logs redis
```

**Port 5012/7045 already in use**
```bash
# Find process
lsof -i :5012
lsof -i :7045

# Kill it
kill -9 <PID>
```

### Redis Connection Errors

**Error**: "It was not possible to connect to the redis server"

**Solution**: Redis is optional. App will work without it.
```bash
# Restart Redis
docker-compose restart redis

# Or disable Redis in Program.cs (already handled)
```

### Kafka Connection Errors

**Error**: "Failed to connect to Kafka"

**Solution**: Wait 30 seconds for Kafka to fully start
```bash
# Check Kafka logs
docker-compose logs kafka

# Restart if needed
docker-compose restart kafka
```

## Environment Variables

### Development
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5012;https://localhost:7045
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:80
```

## Next Steps

1. **Read QUICK_START.md** - Get running in 5 minutes
2. **Read DEVELOPMENT.md** - Development workflow
3. **Read API.md** - API endpoints
4. **Read TESTING.md** - Run tests
5. **Read OBSERVABILITY.md** - Monitor with Kibana

## Support

- **Issues**: Check [GitHub Issues](https://github.com/Mostafa-SAID7/Finance-Api/issues)
- **Documentation**: See `/docs` folder
- **Logs**: Check application console output or Kibana

---

**Last Updated**: April 21, 2026
**Status**: Production Ready
