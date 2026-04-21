# Contributing to FinanceControl

Thank you for your interest in contributing to FinanceControl! This document provides guidelines and instructions for contributing.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions.

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Setup Development Environment

1. **Fork the repository**
   ```bash
   # Click "Fork" on GitHub
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR-USERNAME/Finance-Api.git
   cd FinanceControl
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/Mostafa-SAID7/Finance-Api.git
   ```

4. **Start infrastructure**
   ```bash
   cd src/FinanceControl.Api
   docker-compose up -d
   cd ../..
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/FinanceControl.Api/FinanceControl.Api.csproj
   ```

6. **Run tests**
   ```bash
   dotnet test
   ```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

**Branch naming conventions**:
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test additions

### 2. Make Changes

- Follow the existing code style
- Write clean, readable code
- Add comments for complex logic
- Keep commits atomic and focused

### 3. Write Tests

For every feature or bug fix:

```bash
# Add tests in tests/FinanceControl.IntegrationTests/
# Run tests to verify
dotnet test
```

**Test requirements**:
- Unit tests for business logic
- Integration tests for API endpoints
- Tests should be isolated and repeatable

### 4. Commit Changes

```bash
git add .
git commit -m "type: Brief description"
```

**Commit message format**:
```
type: Brief description (50 chars max)

Longer explanation if needed (wrap at 72 chars)

Fixes #123
```

**Types**:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `test:` - Test additions
- `refactor:` - Code refactoring
- `perf:` - Performance improvement
- `chore:` - Build, dependencies, etc.

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a Pull Request on GitHub with:
- Clear title describing the change
- Description of what changed and why
- Reference to related issues (#123)
- Screenshots if UI changes

## Code Style Guidelines

### C# / .NET

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Keep methods focused and small
- Use async/await for I/O operations
- Add XML documentation comments for public APIs

### Example

```csharp
/// <summary>
/// Adds a new expense to the system.
/// </summary>
/// <param name="request">The expense request details</param>
/// <returns>The created expense response</returns>
public async Task<ExpenseResponse> AddExpenseAsync(ExpenseRequest request)
{
    // Implementation
}
```

### Project Structure

```
src/FinanceControl.Api/
├── Controllers/          # API endpoints
├── Application/          # Business logic & DTOs
├── Domain/               # Domain models & entities
├── Infra/                # Infrastructure services
└── Data/                 # Database & migrations
```

## Testing Guidelines

### Test Structure

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = new TestData();
    
    // Act
    var result = await service.MethodAsync(input);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result.Value);
}
```

### Test Categories

- **Unit Tests**: Test individual methods in isolation
- **Integration Tests**: Test API endpoints with real services
- **Performance Tests**: Test under load conditions

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "TestClassName"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Documentation

### When to Update Documentation

- Adding new features
- Changing configuration
- Fixing bugs that affect usage
- Improving clarity

### Documentation Files

- **README.md** - Project overview
- **docs/QUICK_START.md** - 5-minute setup
- **docs/SETUP_GUIDE.md** - Detailed installation
- **docs/DEVELOPMENT.md** - Development workflow
- **docs/TESTING_GUIDE.md** - Testing guide
- **CHANGELOG.md** - Version history

## Pull Request Process

1. **Update your branch**
   ```bash
   git fetch upstream
   git rebase upstream/master
   ```

2. **Ensure tests pass**
   ```bash
   dotnet test
   ```

3. **Push changes**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create Pull Request**
   - Use the PR template
   - Link related issues
   - Describe changes clearly
   - Request reviewers

5. **Address feedback**
   - Make requested changes
   - Push updates (don't force push)
   - Re-request review

6. **Merge**
   - Maintainers will merge when approved
   - Delete your branch after merge

## Reporting Issues

### Bug Reports

Include:
- Clear description of the bug
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version, etc.)
- Error messages or logs

### Feature Requests

Include:
- Clear description of the feature
- Use case and motivation
- Proposed implementation (optional)
- Examples or mockups (if applicable)

## Getting Help

- 📖 Read [Documentation Index](DOCUMENTATION_INDEX.md)
- 💬 Open a [Discussion](https://github.com/Mostafa-SAID7/Finance-Api/discussions)
- 📋 Check [Troubleshooting Guide](docs/TROUBLESHOOTING.md)
- 🐛 Search [Existing Issues](https://github.com/Mostafa-SAID7/Finance-Api/issues)

## Recognition

Contributors will be recognized in:
- Pull request comments
- Release notes
- Contributors list (coming soon)

## Questions?

Feel free to:
- Open a discussion
- Ask in an issue
- Contact the maintainers

---

Thank you for contributing to FinanceControl! 🎉
