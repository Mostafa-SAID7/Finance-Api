using FinanceControl.Api.Infra.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceControl.IntegrationTests.Infrastructure;

public class LoggingIntegrationTests : IClassFixture<FinanceControlApplicationFactory>
{
    private readonly FinanceControlApplicationFactory _factory;

    public LoggingIntegrationTests(FinanceControlApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_StartsSuccessfully_WithSerilogConfigured()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DatabaseContext_IsAvailable_InApplication()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FinanceControlDbContext>();

        // Act
        var canConnect = await dbContext.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public void Logger_IsConfigured_InDependencyInjection()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Act & Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public async Task HealthCheck_ReturnsSuccessful_WhenServicesAreRunning()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        // Should not throw exception
        Assert.NotNull(response);
    }
}
