# 💰 FinanceControl

> A modern ASP.NET Core API for managing personal finances with integration tests, Docker support, and event-driven architecture.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![Docker](https://img.shields.io/badge/Docker-Supported-2496ED?logo=docker)](https://www.docker.com)
[![Tests](https://img.shields.io/badge/Tests-Integration-green)](docs/TESTING.md)

## 📋 Quick Links

| Documentation | Purpose |
|---|---|
| ⚡ [Quick Start](docs/QUICK_START.md) | Get running in 5 minutes |
| 🔧 [Setup Guide](docs/SETUP_GUIDE.md) | Detailed installation & configuration |
| 🚀 [Getting Started](docs/GETTING_STARTED.md) | Setup and run the application |
| 📚 [Architecture](docs/ARCHITECTURE.md) | System design and components |
| 🔧 [Development Guide](docs/DEVELOPMENT.md) | Contributing and development workflow |
| 🧪 [Testing Guide](docs/TESTING_GUIDE.md) | Testing strategies and examples |
| 📊 [Observability](docs/OBSERVABILITY.md) | Monitoring with Kibana & Elasticsearch |
| 🐳 [Deployment](docs/DEPLOYMENT.md) | Docker and production deployment |
| 📡 [API Reference](docs/API.md) | Available endpoints and usage |
| 🆘 [Troubleshooting](docs/TROUBLESHOOTING.md) | Common issues and solutions |

## ✨ Features

- **Modern Stack**: Built with .NET 9 and ASP.NET Core
- **Event-Driven**: Kafka integration for event publishing
- **Database**: PostgreSQL with Entity Framework Core
- **Testing**: Comprehensive integration tests with Testcontainers
- **API Documentation**: Swagger/OpenAPI support
- **Docker Ready**: Multi-stage Dockerfile with optimized builds
- **CI/CD**: GitHub Actions workflows for automated testing and deployment
- **Clean Architecture**: Layered design with separation of concerns

## 🏗️ Architecture

```
Controllers (API Layer)
    ↓
Application Layer (Business Logic)
    ↓
Domain Layer (Entities & Rules)
    ↓
Infrastructure (Database & Events)
```

**Key Components**:
- **BalanceController**: Query financial balance
- **ExpensesController**: Manage expenses
- **IncomesController**: Manage incomes
- **PostgreSQL**: Data persistence
- **Kafka**: Event streaming

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Setup (5 minutes)

```bash
# Clone repository
git clone https://github.com/Mostafa-SAID7/Finance-Api.git
cd FinanceControl

# Start infrastructure
cd src/FinanceControl.Api
docker-compose up -d

# Run application
cd ../..
dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
```

**Access the API**:
- 🌐 API: `http://localhost:5012`
- 📖 Swagger: `http://localhost:5012/swagger`
- 📊 Kibana (Logs): `http://localhost:5601`

**For detailed setup**: See [Quick Start Guide](docs/QUICK_START.md) or [Setup Guide](docs/SETUP_GUIDE.md)

## 📦 Project Structure

```
Finance-Api/
├── src/
│   └── FinanceControl.Api/
│       ├── Controllers/          # API endpoints
│       ├── Application/          # Business logic
│       ├── Domain/               # Domain models
│       ├── Infra/                # Infrastructure services
│       └── Data/                 # Database & migrations
├── tests/
│   └── FinanceControl.IntegrationTests/
│       └── Controllers/          # Integration tests
├── docs/                         # Documentation
└── .github/workflows/            # CI/CD pipelines
```

## 🔌 API Endpoints

### Balance
```http
GET /api/balance
```

### Expenses
```http
POST /api/expenses
Content-Type: application/json

{
  "description": "Grocery shopping",
  "amount": 50.00,
  "date": "2026-04-21"
}
```

### Incomes
```http
POST /api/incomes
Content-Type: application/json

{
  "description": "Salary",
  "amount": 3000.00,
  "date": "2026-04-21"
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "TestClassName"
```

**Test Status**: 18/22 passing ✅
- Controllers: 2/2 ✅
- Cache Service: 7/7 ✅
- Redis Integration: 9/9 ✅
- Logging: 4/4 (Docker timeout - infrastructure issue)

Tests use **Testcontainers** to automatically provision PostgreSQL, Kafka, and Redis for isolated testing.

**For detailed testing info**: See [Testing Guide](docs/TESTING_GUIDE.md)

## 🐳 Docker

### Build Image
```bash
docker build -f src/FinanceControl.Api/Dockerfile -t finance-api:latest .
```

### Run Container
```bash
docker run -d \
  -p 5000:80 \
  -e ConnectionStrings__Postgres="Host=postgres:5432;Database=FinanceControl;Username=postgres;Password=admin" \
  -e Kafka__BootstrapServers="kafka:9092" \
  finance-api:latest
```

### Docker Compose
```bash
docker-compose up -d
```

## 🔄 CI/CD Pipeline

The project includes automated workflows:

- **Docker Image CI**: Builds and pushes Docker images to GitHub Container Registry (GHCR)
- **Test Automation**: Runs integration tests on every push and PR
- **Image Tagging**: Automatic semantic versioning and branch-based tags

**Image Registry**: `ghcr.io/mostafa-said7/finance-api`

## 📊 Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Runtime | .NET 9 | Application runtime |
| Web Framework | ASP.NET Core | API framework |
| ORM | Entity Framework Core 9.0 | Database access |
| Database | PostgreSQL 15 | Data persistence |
| Message Broker | Kafka | Event streaming |
| Cache | Redis | Performance caching |
| Logging | Serilog | Structured logging |
| Log Storage | Elasticsearch | Log aggregation |
| Log Viewer | Kibana | Log visualization |
| Testing | xUnit, Testcontainers | Integration testing |
| API Docs | Swagger/OpenAPI | API documentation |
| Containerization | Docker | Application packaging |
| CI/CD | GitHub Actions | Automated workflows |

## 🛠️ Development

### Add New Feature
1. Create domain model in `Domain/`
2. Create database migration
3. Implement business logic in `Application/`
4. Create controller endpoint in `Controllers/`
5. Add integration tests
6. Submit pull request

See [Development Guide](docs/DEVELOPMENT.md) for detailed instructions.

### Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName --project src/FinanceControl.Api

# Apply migrations (automatic on startup)
dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
```

## 📝 Configuration

### Development (`appsettings.Development.json`)
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

### Production
Use environment variables or secrets manager for sensitive data.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [Development Guide](docs/DEVELOPMENT.md) for contribution guidelines.

## 📖 Documentation

Complete documentation for all aspects of the project:

**Getting Started**:
- **[Quick Start](docs/QUICK_START.md)** - Get running in 5 minutes
- **[Setup Guide](docs/SETUP_GUIDE.md)** - Detailed installation & configuration
- **[Getting Started](docs/GETTING_STARTED.md)** - Initial setup and first run

**Development**:
- **[Architecture](docs/ARCHITECTURE.md)** - System design and components
- **[Development](docs/DEVELOPMENT.md)** - Development workflow and guidelines
- **[Testing Guide](docs/TESTING_GUIDE.md)** - Testing strategies and examples

**Operations**:
- **[Observability](docs/OBSERVABILITY.md)** - Monitoring with Kibana & Elasticsearch
- **[Redis Guide](docs/REDIS.md)** - Redis caching configuration
- **[Logging Guide](docs/LOGGING.md)** - Serilog logging setup
- **[Deployment](docs/DEPLOYMENT.md)** - Docker and production deployment

**Reference**:
- **[API Reference](docs/API.md)** - Available endpoints and usage
- **[Troubleshooting](docs/TROUBLESHOOTING.md)** - Common issues and solutions
- **[Test Results](TEST_RESULTS_SUMMARY.md)** - Current test status and coverage
- **[Changelog](CHANGELOG.md)** - Version history and changes

## 🐛 Troubleshooting

### Database Connection Issues
- Verify PostgreSQL is running: `docker ps`
- Check connection string in `appsettings.Development.json`

### Kafka Connection Issues
- Verify Kafka is running: `docker ps`
- Check bootstrap servers configuration

See [Troubleshooting Guide](docs/TROUBLESHOOTING.md) for more solutions.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Based on [Live #69: Implementando Testes de Integração em .NET com Docker](https://www.youtube.com/live/o5Q73A-rrlg?feature=share)
- Original repository: [bufaonanet/FinanceControl](https://github.com/bufaonanet/FinanceControl)

## 📞 Support

For issues and questions:
- 📋 [Open an Issue](https://github.com/Mostafa-SAID7/Finance-Api/issues)
- 💬 [Discussions](https://github.com/Mostafa-SAID7/Finance-Api/discussions)

---

**Made with ❤️ by [Mostafa SAID](https://github.com/Mostafa-SAID7)**
