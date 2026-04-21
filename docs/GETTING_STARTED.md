# Getting Started with FinanceControl

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/Mostafa-SAID7/Finance-Api.git
cd Finance-Api
```

### 2. Start Infrastructure Services

The application requires PostgreSQL and Kafka. Start them using Docker Compose:

```bash
cd src/FinanceControl.Api
docker-compose up -d
```

This will start:
- **PostgreSQL** on `localhost:5432`
- **Kafka** on `localhost:9092`

### 3. Apply Database Migrations

Migrations are applied automatically when the application starts via the `MigrateDatabaseHostedService`.

### 4. Run the Application

```bash
dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

### 5. Run Integration Tests

```bash
dotnet test
```

## Environment Configuration

The application uses `appsettings.Development.json` for local development:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost:5432;Database=FinanceControl;Username=postgres;Password=admin"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

## Next Steps

- Read the [API Documentation](./API.md) to understand available endpoints
- Check [Architecture](./ARCHITECTURE.md) for system design details
- Review [Development Guide](./DEVELOPMENT.md) for contribution guidelines
