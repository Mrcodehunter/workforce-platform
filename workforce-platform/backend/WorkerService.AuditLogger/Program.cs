using MongoDB.Driver;
using Serilog;
using WorkerService.AuditLogger;
using WorkerService.AuditLogger.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Workforce.Shared.Cache;
using Workforce.Shared.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [AuditWorker] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

// MongoDB Configuration
var mongoConnection = builder.Configuration.GetConnectionString("MongoDB");
var mongoClient = new MongoClient(mongoConnection);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);

// Redis Configuration (from shared library)
builder.Services.AddRedisCache(builder.Configuration);

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
