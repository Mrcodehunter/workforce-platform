using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Scalar.AspNetCore;
using Serilog;
using WorkforceAPI;
using WorkforceAPI.Data;
using WorkforceAPI.Repositories;
using WorkforceAPI.Services;
using Workforce.Shared.Cache;
using Workforce.Shared.DependencyInjection;
using Workforce.Shared.EventPublisher;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references by ignoring cycles
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 32;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Database Configuration
// PostgreSQL
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<WorkforceDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// MongoDB
var mongoConnection = builder.Configuration.GetConnectionString("MongoDB");
var mongoClient = new MongoClient(mongoConnection);
var mongoDatabase = mongoClient.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);

// Database Seeder
builder.Services.AddScoped<DatabaseSeeder>();

// Redis Cache (from shared library)
builder.Services.AddRedisCache(builder.Configuration);

// RabbitMQ Event Publisher (from shared library)
builder.Services.AddRabbitMqPublisher();

// Repositories
builder.Services.AddRepositories();

// Services
builder.Services.AddApplicationServices();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Map OpenAPI endpoint
    app.MapOpenApi();
    
    // Map Scalar API documentation UI at root path
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Workforce Management API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
    
    // Redirect root to Scalar documentation
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

// CORS must be before UseAuthorization and MapControllers
app.UseCors("AllowFrontend");

// Global exception handler
app.UseMiddleware<WorkforceAPI.Middleware.GlobalExceptionHandlerMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
}));

// Initialize database (migrations and seeding)
await app.Services.SeedDatabaseAsync();

Log.Information("Starting Workforce Management API");

app.Run();
