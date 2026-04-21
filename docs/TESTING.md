# Testing Guide

## Testing Strategy

FinanceControl uses a comprehensive testing approach with integration tests as the primary focus.

## Test Types

### Unit Tests
- Test individual components in isolation
- Mock external dependencies
- Fast execution
- Currently minimal (focus on integration tests)

### Integration Tests
- Test API endpoints with real database and Kafka
- Use Testcontainers for isolated environments
- Verify end-to-end workflows
- Primary testing approach

### End-to-End Tests
- Test complete user workflows
- Can be added for critical paths
- Slower execution

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "ClassName"
```

### Run Specific Test Method
```bash
dotnet test --filter "ClassName.MethodName"
```

### Run with Verbose Output
```bash
dotnet test --verbosity detailed
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Integration Test Structure

### Test Base Class

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; }
    protected FinanceControlDbContext DbContext { get; private set; }
    
    private WebApplicationFactory<Program> _factory;
    
    public async Task InitializeAsync()
    {
        _factory = new FinanceControlApplicationFactory();
        Client = _factory.CreateClient();
        DbContext = _factory.Services.GetRequiredService<FinanceControlDbContext>();
    }
    
    public async Task DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
    }
}
```

### Test Example

```csharp
public class ExpensesControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateExpense_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            Description = "Grocery shopping",
            Amount = 50.00m,
            Date = DateTime.Now
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/expenses", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsAsync<ExpenseResponse>();
        content.Description.Should().Be(request.Description);
    }
    
    [Fact]
    public async Task CreateExpense_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExpenseRequest
        {
            Description = "",
            Amount = -50.00m
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/expenses", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Testcontainers

### PostgreSQL Container

```csharp
private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
    .WithImage("postgres:15")
    .WithDatabase("FinanceControl")
    .WithUsername("postgres")
    .WithPassword("admin")
    .Build();

public async Task InitializeAsync()
{
    await _postgres.StartAsync();
    var connectionString = _postgres.GetConnectionString();
}
```

### Kafka Container

```csharp
private readonly KafkaContainer _kafka = new KafkaBuilder()
    .WithImage("confluentinc/cp-kafka:7.5.0")
    .Build();

public async Task InitializeAsync()
{
    await _kafka.StartAsync();
    var bootstrapServers = _kafka.GetBootstrapServers();
}
```

## Test Data Management

### Database Seeding

```csharp
private async Task SeedTestData()
{
    var expense = new Expense
    {
        Description = "Test expense",
        Amount = 100.00m,
        Date = DateTime.Now
    };
    
    DbContext.Expenses.Add(expense);
    await DbContext.SaveChangesAsync();
}
```

### Database Cleanup

```csharp
private async Task CleanupDatabase()
{
    DbContext.Expenses.RemoveRange(DbContext.Expenses);
    DbContext.Incomes.RemoveRange(DbContext.Incomes);
    await DbContext.SaveChangesAsync();
}
```

## Assertions

Using FluentAssertions for readable assertions:

```csharp
response.StatusCode.Should().Be(HttpStatusCode.OK);
content.Should().NotBeNull();
content.Amount.Should().BeGreaterThan(0);
content.Description.Should().Contain("Test");
```

## Mocking

### Mock DbContext

```csharp
var mockContext = new Mock<FinanceControlDbContext>();
mockContext.Setup(x => x.Expenses.Add(It.IsAny<Expense>()))
    .Callback<Expense>(e => expenses.Add(e));
```

### Mock External Services

```csharp
var mockBrokerService = new Mock<BrokerService>();
mockBrokerService.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
    .Returns(Task.CompletedTask);
```

## Test Coverage

### Generate Coverage Report

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:Exclude="[*]*.Program"
```

### Coverage Goals

- **Overall**: Aim for 80%+ coverage
- **Critical Paths**: 100% coverage
- **Controllers**: 90%+ coverage
- **Business Logic**: 85%+ coverage

## Continuous Integration

Tests run automatically on:
- Every push to any branch
- Every pull request
- Scheduled daily runs

### GitHub Actions Workflow

```yaml
- name: Run Tests
  run: dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"

- name: Upload Test Results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-results
    path: '**/test-results.trx'
```

## Performance Testing

### Load Testing with k6

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 10,
  duration: '30s',
};

export default function () {
  let response = http.get('https://localhost:5001/api/balance');
  check(response, {
    'status is 200': (r) => r.status === 200,
  });
}
```

Run: `k6 run load-test.js`

## Debugging Tests

### Debug in Visual Studio
1. Set breakpoint in test
2. Right-click test → Debug
3. Use Debug menu for step-through

### Debug in VS Code
1. Add breakpoint
2. Run test with debugger
3. Use Debug Console

### Verbose Logging

```csharp
[Fact]
public async Task MyTest()
{
    _output.WriteLine("Test started");
    // Test code
    _output.WriteLine("Test completed");
}
```

## Best Practices

1. **Arrange-Act-Assert**: Follow AAA pattern
2. **One Assertion**: Focus on one behavior per test
3. **Descriptive Names**: Use clear test names
4. **Isolation**: Tests should be independent
5. **Cleanup**: Always clean up resources
6. **No Hardcoding**: Use test data builders
7. **Fast Execution**: Keep tests fast
8. **Deterministic**: Tests should always pass/fail consistently

## Troubleshooting

### Tests Timeout
- Increase timeout: `[Fact(Timeout = 5000)]`
- Check for deadlocks
- Verify container startup

### Container Issues
- Check Docker is running
- Verify sufficient disk space
- Check container logs

### Database Issues
- Verify migrations run
- Check connection string
- Verify database cleanup between tests
