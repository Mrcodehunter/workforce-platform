using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;
using WorkforceAPI.Data;
using WorkforceAPI.EventPublisher;
using WorkforceAPI.Repositories;
using WorkforceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

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

// RabbitMQ Event Publisher
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workforce Management API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow 
}));

Log.Information("Starting Workforce Management API");

app.Run();
