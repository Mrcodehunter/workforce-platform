using Serilog;
using WorkerService.AuditLogger;
using WorkerService.AuditLogger.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Workforce.Shared.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [AuditWorker] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

// MongoDB Configuration (using shared infrastructure extension)
builder.Services.AddMongoDatabase(builder.Configuration);

// Shared Infrastructure (Redis, RabbitMQ) - centralized DI with environment awareness
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Services
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IRabbitMqConsumer, RabbitMqConsumer>();

// Background Worker
builder.Services.AddHostedService<AuditWorker>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<WorkerHealthCheck>("worker_health");

var host = builder.Build();

Log.Information("Starting Audit Logger Worker Service");

host.Run();
