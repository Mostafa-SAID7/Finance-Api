using FinanceControl.Api.Infra;
using FinanceControl.Api.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Enrichers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
            new Uri(context.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = "finance-api-{0:yyyy.MM.dd}",
            MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
        });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Redis with graceful fallback
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
try
{
    var options = ConfigurationOptions.Parse(redisConnection);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 5000;
    options.SyncTimeout = 5000;
    
    var multiplexer = ConnectionMultiplexer.Connect(options);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    builder.Services.AddScoped<ICacheService, CacheService>();
    Log.Information("Redis connection configured successfully");
}
catch (Exception ex)
{
    Log.Warning(ex, "Failed to configure Redis. Cache service will be disabled. Redis connection: {RedisConnection}", redisConnection);
    // Register a no-op cache service if Redis is unavailable
    builder.Services.AddScoped<ICacheService, NoCacheService>();
}

// Add Database
builder.Services.AddDbContext<FinanceControlDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Add Kafka
builder.Services.Configure<BrokerOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddScoped<BrokerService>();
builder.Services.AddHostedService<MigrateDatabaseHostedService>();    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Configure 404 fallback for static pages
app.Use(async (context, next) =>
{
    await next();
    
    // If the response is 404 and the request is for a page (not an API endpoint or static file)
    if (context.Response.StatusCode == 404 && !context.Request.Path.StartsWithSegments("/api"))
    {
        context.Request.Path = "/404.html";
        await next();
    }
});

app.Run();

public partial class Program { }