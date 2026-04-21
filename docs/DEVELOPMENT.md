# Development Guide

## Project Structure

```
FinanceControl/
├── src/
│   └── FinanceControl.Api/
│       ├── Application/          # Business logic & use cases
│       ├── Controllers/          # API endpoints
│       ├── Domain/               # Domain models & entities
│       ├── Infra/                # Infrastructure services
│       ├── Data/                 # Database context & migrations
│       └── Properties/           # Project configuration
├── tests/
│   └── FinanceControl.IntegrationTests/
│       └── Controllers/          # Integration tests
├── docs/                         # Documentation
└── .github/workflows/            # CI/CD pipelines
```

## Development Workflow

### 1. Setup Development Environment

```bash
# Clone repository
git clone https://github.com/Mostafa-SAID7/Finance-Api.git
cd Finance-Api

# Start infrastructure
cd src/FinanceControl.Api
docker-compose up -d

# Restore dependencies
dotnet restore

# Run application
dotnet run
```

### 2. Code Style & Conventions

- **Language**: C# 12+
- **Naming**: PascalCase for classes, camelCase for variables
- **Async**: Use async/await for I/O operations
- **Null Safety**: Enable nullable reference types
- **Comments**: Document complex logic and public APIs

### 3. Adding New Features

#### Step 1: Create Domain Model
```csharp
// Domain/YourFeature/YourEntity.cs
public class YourEntity : BaseEntity
{
    public string Name { get; set; }
    // Add properties
}
```

#### Step 2: Create Database Migration
```bash
dotnet ef migrations add AddYourEntity --project src/FinanceControl.Api
```

#### Step 3: Create Application Logic
```csharp
// Application/YourFeature/YourService.cs
public class YourService
{
    private readonly FinanceControlDbContext _context;
    
    public async Task CreateAsync(YourEntity entity)
    {
        _context.YourEntities.Add(entity);
        await _context.SaveChangesAsync();
    }
}
```

#### Step 4: Create Controller Endpoint
```csharp
// Controllers/YourController.cs
[ApiController]
[Route("api/[controller]")]
public class YourController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourRequest request)
    {
        // Implementation
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

#### Step 5: Add Integration Tests
```csharp
// tests/FinanceControl.IntegrationTests/Controllers/YourControllerTests.cs
public class YourControllerTests : IAsyncLifetime
{
    [Fact]
    public async Task Create_WithValidData_ReturnsCreated()
    {
        // Arrange, Act, Assert
    }
}
```

### 4. Database Migrations

#### Create Migration
```bash
dotnet ef migrations add MigrationName --project src/FinanceControl.Api
```

#### Apply Migrations
Migrations are applied automatically on application startup via `MigrateDatabaseHostedService`.

#### Revert Migration
```bash
dotnet ef migrations remove --project src/FinanceControl.Api
```

### 5. Testing

#### Run All Tests
```bash
dotnet test
```

#### Run Specific Test
```bash
dotnet test --filter "ClassName"
```

#### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

#### Integration Test Setup
Tests use Testcontainers to spin up PostgreSQL and Kafka automatically:

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .Build();
    
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}
```

### 6. Debugging

#### Debug in Visual Studio
1. Set breakpoints in code
2. Press `F5` to start debugging
3. Use Debug menu for step-through debugging

#### Debug in VS Code
1. Install C# extension
2. Create `.vscode/launch.json`
3. Press `F5` to start debugging

### 7. Common Tasks

#### Add NuGet Package
```bash
dotnet add package PackageName --project src/FinanceControl.Api
```

#### Update NuGet Packages
```bash
dotnet list package --outdated
dotnet package update --project src/FinanceControl.Api
```

#### Format Code
```bash
dotnet format
```

#### Analyze Code
```bash
dotnet build /p:EnforceCodeStyleInBuild=true
```

## Git Workflow

### Branch Naming
- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation updates

### Commit Messages
```
[TYPE] Brief description

Detailed explanation if needed.

Fixes #123
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Pull Request Process
1. Create feature branch
2. Make changes and commit
3. Push to remote
4. Create Pull Request
5. Wait for CI/CD checks
6. Request review
7. Merge after approval

## Troubleshooting

### Database Connection Issues
- Verify PostgreSQL is running: `docker ps`
- Check connection string in `appsettings.Development.json`
- Ensure database exists: `docker exec -it postgres psql -U postgres -l`

### Kafka Connection Issues
- Verify Kafka is running: `docker ps`
- Check bootstrap servers in configuration
- Test connection: `docker exec -it kafka kafka-broker-api-versions.sh --bootstrap-server localhost:9092`

### Migration Failures
- Check migration files for syntax errors
- Verify database state: `dotnet ef migrations list`
- Rollback if needed: `dotnet ef migrations remove`

## Performance Optimization

- Use async/await for I/O operations
- Implement database query optimization
- Add caching for frequently accessed data
- Monitor application performance with Application Insights
- Use connection pooling for database connections

## Security Best Practices

- Validate all user inputs
- Use parameterized queries (EF Core handles this)
- Implement authentication and authorization
- Use HTTPS in production
- Secure sensitive configuration with secrets manager
- Regular dependency updates for security patches
