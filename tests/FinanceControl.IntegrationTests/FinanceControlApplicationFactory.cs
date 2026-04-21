using FinanceControl.Api.Infra;
using FinanceControl.Api.Infra.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using StackExchange.Redis;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace FinanceControl.IntegrationTests;

public class FinanceControlApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder().Build();
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder().Build();
    private readonly RedisContainer _redisContainer = new RedisBuilder().Build();


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextOptionsDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<FinanceControlDbContext>));
            
            var brokerOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(BrokerOptions)
            );
            
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer)
            );
            
            services.Remove(dbContextOptionsDescriptor!);
            services.Remove(brokerOptionsDescriptor!);
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);
            
            services.AddDbContext<FinanceControlDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });
            services.Configure<BrokerOptions>(options =>
            {
                options.BootstrapServers = _kafkaContainer.GetBootstrapAddress();
            });
            
            // Add Redis from test container
            var redisConnectionString = _redisContainer.GetConnectionString();
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            var multiplexer = ConnectionMultiplexer.Connect(options);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddScoped<ICacheService, CacheService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _kafkaContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _kafkaContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
    
    public async Task ClearDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });

        await respawner.ResetAsync(connection);
    }
    
    public string GetBootstrapAddress()
    {
        return _kafkaContainer.GetBootstrapAddress();
    }
}