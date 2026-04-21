# Quick Start Guide

Get the FinanceControl API running in 5 minutes.

## Prerequisites

- .NET 9 SDK
- Docker & Docker Compose
- Git

## 1. Clone & Setup (1 minute)

```bash
git clone https://github.com/Mostafa-SAID7/Finance-Api.git
cd FinanceControl
```

## 2. Start Infrastructure (2 minutes)

```bash
cd src/FinanceControl.Api
docker-compose up -d
```

This starts:
- **PostgreSQL** (port 5432) - Database
- **Kafka** (port 9092) - Message broker
- **Redis** (port 6379) - Cache (optional)
- **Elasticsearch** (port 9200) - Logging
- **Kibana** (port 5601) - Log viewer

## 3. Run Application (1 minute)

```bash
dotnet run
```

Application starts on:
- **HTTP**: http://localhost:5012
- **HTTPS**: https://localhost:7045
- **Swagger**: http://localhost:5012/swagger

## 4. Test It Works (1 minute)

```bash
# Create an expense
curl -X POST http://localhost:5012/expenses \
  -H "Content-Type: application/json" \
  -d '{
    "value": 50.00,
    "type": 1,
    "date": "2024-04-21T00:00:00Z",
    "isRecurrent": false
  }'

# Get balance
curl http://localhost:5012/balance
```

## Stopping Everything

```bash
# Stop application
# Press Ctrl+C in terminal

# Stop containers
docker-compose down
```

## Troubleshooting

### Port Already in Use
```bash
# Find what's using port 5432
lsof -i :5432

# Kill it
kill -9 <PID>
```

### Redis Connection Error
- Redis is optional - app works without it
- Check if container is running: `docker ps`
- Restart: `docker-compose restart redis`

### Database Migration Failed
```bash
# Reset database
docker-compose down -v
docker-compose up -d
dotnet run
```

### Can't Connect to Kafka
- Wait 30 seconds for Kafka to fully start
- Check logs: `docker-compose logs kafka`

## Next Steps

- Read [DEVELOPMENT.md](./DEVELOPMENT.md) for development workflow
- Read [TESTING.md](./TESTING.md) for running tests
- Read [API.md](./API.md) for API endpoints
- Read [OBSERVABILITY.md](./OBSERVABILITY.md) for monitoring

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                  FinanceControl API                 │
│                   (.NET 9 ASP.NET)                  │
└─────────────────────────────────────────────────────┘
         ↓              ↓              ↓
    ┌────────┐    ┌─────────┐    ┌────────┐
    │   DB   │    │  Kafka  │    │ Redis  │
    │ Postgres│   │ (Events)│    │(Cache) │
    └────────┘    └─────────┘    └────────┘
         ↓
    ┌──────────────────────┐
    │  Elasticsearch       │
    │  (Logs)              │
    └──────────────────────┘
         ↓
    ┌──────────────────────┐
    │  Kibana              │
    │  (Log Viewer)        │
    └──────────────────────┘
```

---

**Time to first request**: ~5 minutes
**All services healthy**: ~30 seconds after startup
