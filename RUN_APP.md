# How to Run FinanceControl API

## Prerequisites

- .NET 9 SDK installed
- Docker and Docker Compose installed
- Git installed

## Quick Start (5 minutes)

### Step 1: Start Docker Services

```bash
cd src/FinanceControl.Api
docker-compose up -d
```

This starts:
- PostgreSQL (port 5432)
- Kafka (port 9092)
- Redis (port 6379)
- Elasticsearch (port 9200)
- Kibana (port 5601)

**Verify services are running:**
```bash
docker ps
```

### Step 2: Run the Application

From the project root:

```bash
dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
```

Or from the API directory:

```bash
cd src/FinanceControl.Api
dotnet run
```

### Step 3: Access the Application

Once the app starts, you'll see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7045
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5012
```

**Open in your browser:**

| Page | URL |
|------|-----|
| 🏠 Home | http://localhost:5012 |
| 📖 API Docs | http://localhost:5012/docs |
| 🔗 Swagger UI | http://localhost:5012/swagger |
| 📊 Kibana Logs | http://localhost:5601 |

## Troubleshooting

### Connection Refused Error

**Problem:** `ERR_CONNECTION_REFUSED` when accessing localhost:5012

**Solution:**
1. Make sure the app is running (you should see "Now listening on" messages)
2. Check if port 5012 is available: `netstat -ano | findstr :5012` (Windows)
3. Wait 5-10 seconds after starting for the app to fully initialize
4. Try accessing http://localhost:5012 (not https)

### Docker Services Not Running

**Problem:** Can't connect to PostgreSQL, Redis, or Kafka

**Solution:**
```bash
# Check running containers
docker ps

# Start services if not running
cd src/FinanceControl.Api
docker-compose up -d

# View logs
docker-compose logs -f
```

### Database Connection Error

**Problem:** "Unable to connect to PostgreSQL"

**Solution:**
```bash
# Verify PostgreSQL is running
docker ps | grep postgres

# Check connection string in appsettings.Development.json
# Should be: Host=localhost:5432;Database=FinanceControl;Username=postgres;Password=admin

# Test connection
psql -h localhost -U postgres -d FinanceControl
```

### Port Already in Use

**Problem:** "Address already in use" error

**Solution:**
```bash
# Find process using port 5012
netstat -ano | findstr :5012

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or use a different port by setting environment variable
set ASPNETCORE_URLS=http://localhost:5013
dotnet run
```

## Development Workflow

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/FinanceControl.IntegrationTests

# Run with verbose output
dotnet test --verbosity detailed
```

### Building the Project

```bash
# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release
```

### Viewing Logs

**In Kibana:**
1. Open http://localhost:5601
2. Go to "Discover"
3. Select "finance-api-*" index pattern
4. View logs in real-time

**In Console:**
- Logs appear in the terminal where you ran `dotnet run`

## API Endpoints

### Balance
- `GET /api/balance` - Get current balance

### Expenses
- `POST /api/expenses` - Create expense
- `GET /api/expenses` - List all expenses
- `GET /api/expenses/{id}` - Get specific expense
- `PUT /api/expenses/{id}` - Update expense
- `DELETE /api/expenses/{id}` - Delete expense

### Incomes
- `POST /api/incomes` - Create income
- `GET /api/incomes` - List all incomes
- `GET /api/incomes/{id}` - Get specific income
- `PUT /api/incomes/{id}` - Update income
- `DELETE /api/incomes/{id}` - Delete income

## Stopping the Application

### Stop the API
Press `Ctrl+C` in the terminal where the app is running

### Stop Docker Services
```bash
cd src/FinanceControl.Api
docker-compose down
```

### Stop and Remove All Data
```bash
cd src/FinanceControl.Api
docker-compose down -v
```

## Environment Variables

You can override settings using environment variables:

```bash
# Set custom port
set ASPNETCORE_URLS=http://localhost:5013

# Set environment
set ASPNETCORE_ENVIRONMENT=Production

# Set database connection
set ConnectionStrings__Postgres=Host=myhost;Database=mydb;Username=user;Password=pass

# Run with custom settings
dotnet run
```

## Performance Tips

1. **Use Redis Caching**: Frequently accessed data is cached automatically
2. **Check Logs in Kibana**: Monitor performance and errors in real-time
3. **Run Tests**: Ensure changes don't break existing functionality
4. **Use Swagger UI**: Test endpoints interactively at http://localhost:5012/swagger

## Next Steps

- Read the [API Documentation](docs/API.md)
- Check the [Architecture Guide](docs/ARCHITECTURE.md)
- Review [Development Guidelines](docs/DEVELOPMENT.md)
- Explore [Testing Guide](docs/TESTING_GUIDE.md)

---

**Need help?** Check the [Troubleshooting Guide](docs/TROUBLESHOOTING.md) or open an issue on GitHub.
