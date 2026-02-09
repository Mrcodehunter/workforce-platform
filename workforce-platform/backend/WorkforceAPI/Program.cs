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

// Configure Serilog for structured logging
// Serilog provides better logging capabilities than the default .NET logger
// Configuration is read from appsettings.json (Serilog section)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

// Use Serilog as the logging provider for the entire application
builder.Host.UseSerilog();

// Add services to the container
// Configure JSON serialization options to handle circular references
// This is necessary because Entity Framework navigation properties create circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references by ignoring cycles
        // When serializing Employee -> Department -> Employees, the cycle is broken
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 32;  // Maximum nesting depth for safety
        options.JsonSerializerOptions.WriteIndented = false;  // Compact JSON (smaller payload)
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();  // Required for Scalar API documentation

// FluentValidation - Server-side validation
// Validators are automatically discovered from the assembly and registered
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// Enable automatic validation - FluentValidation automatically validates models before action execution
builder.Services.AddFluentValidationAutoValidation();
// Enable client-side validation adapters (for ASP.NET Core MVC, not used in API but harmless)
builder.Services.AddFluentValidationClientsideAdapters();

// Database Configuration
// PostgreSQL - Primary relational database for entities (Employee, Project, Task, etc.)
var postgresConnection = builder.Configuration.GetConnectionString("PostgreSQL");
// Apply environment-specific defaults if connection string is not configured
if (string.IsNullOrEmpty(postgresConnection))
{
    var environmentName = builder.Environment.EnvironmentName;
    // Development: localhost (for local PostgreSQL instance)
    // Production: postgres (Docker service name in docker-compose)
    postgresConnection = environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
        ? "Host=localhost;Port=5432;Database=workforce_db;Username=postgres;Password=postgres"
        : "Host=postgres;Port=5432;Database=workforce_db;Username=admin;Password=changeme";
}

// Register Entity Framework Core DbContext with PostgreSQL provider
// DbContext is registered as Scoped (one per HTTP request)
builder.Services.AddDbContext<WorkforceDbContext>(options =>
    options.UseNpgsql(postgresConnection));

// MongoDB - Document database for audit logs and leave requests
// Uses shared infrastructure extension from Workforce.Shared
// Provides environment-aware configuration (localhost for dev, mongodb for prod)
builder.Services.AddMongoDatabase(builder.Configuration);

// Database Seeder - Used to populate database with sample data
// Registered as Scoped to match DbContext lifetime
builder.Services.AddScoped<DatabaseSeeder>();

// Shared Infrastructure (Redis, RabbitMQ) - centralized DI with environment awareness
// This extension method registers:
// - Redis cache (for audit trail snapshots)
// - RabbitMQ publisher (for domain events)
// Both with environment-specific configuration
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Repositories - Data access layer
// All repositories are registered with Scoped lifetime
builder.Services.AddRepositories();

// Services - Business logic layer
// All services are registered with Scoped lifetime
builder.Services.AddApplicationServices();

// AutoMapper - Object-to-object mapping
// Automatically discovers and registers mapping profiles from the assembly
// Used for converting between entities and DTOs (if needed)
builder.Services.AddAutoMapper(typeof(Program));

// CORS - Cross-Origin Resource Sharing
// Allows the frontend application (running on a different port/domain) to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Allow requests from frontend URL (default: http://localhost:3000)
        policy.WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:3000")
              .AllowAnyHeader()  // Allow all HTTP headers
              .AllowAnyMethod();  // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Middleware order is important - they execute in the order they are added

// OpenAPI and Scalar API Documentation
// Map OpenAPI/Swagger JSON endpoint
// This generates the OpenAPI specification from controller attributes and DTOs
// Accessible at /openapi/v1.json
app.MapOpenApi();

// Map Scalar API documentation UI
// Scalar is a modern alternative to Swagger UI with better UX
// Accessible at /scalar/v1 when running in Docker: http://localhost:5000/scalar/v1
// The documentation is available in all environments for easy API exploration
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Workforce Management API")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// Redirect root path to Scalar documentation for convenience
// When accessing http://localhost:5000/, it redirects to /scalar/v1
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// CORS must be before UseAuthorization and MapControllers
// This allows cross-origin requests from the frontend
app.UseCors("AllowFrontend");

// Global exception handler middleware
// Catches all unhandled exceptions and converts them to consistent JSON error responses
// Must be early in the pipeline to catch exceptions from all subsequent middleware
app.UseMiddleware<WorkforceAPI.Middleware.GlobalExceptionHandlerMiddleware>();

// Authorization middleware (currently not configured, but placeholders for future auth)
app.UseAuthorization();

// Map all controller endpoints
// Controllers are discovered automatically from the Controllers folder
app.MapControllers();

// Health check endpoint
// Used by load balancers, monitoring systems, and Docker health checks
// Returns 200 OK with status and timestamp
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
}));

// Initialize database (create schema and seed sample data)
// This runs once at application startup
// In production, use migrations instead of EnsureCreatedAsync
await app.Services.SeedDatabaseAsync();

Log.Information("Starting Workforce Management API");

// Start the web server and begin listening for HTTP requests
app.Run();
