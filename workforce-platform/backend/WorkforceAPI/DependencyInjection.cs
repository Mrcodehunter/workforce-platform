using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;
using WorkforceAPI.Data;
using WorkforceAPI.Repositories;
using WorkforceAPI.Services;

namespace WorkforceAPI;

/// <summary>
/// Centralized dependency injection configuration for WorkforceAPI
/// </summary>
/// <remarks>
/// This static class provides extension methods for registering all application services.
/// It follows the Single Responsibility Principle by separating DI configuration from Program.cs.
/// 
/// All services are registered with Scoped lifetime, meaning:
/// - One instance per HTTP request
/// - Shared across services within the same request
/// - Disposed at the end of the request
/// 
/// This is appropriate for:
/// - Repositories (database context per request)
/// - Services (business logic per request)
/// - Any service that depends on DbContext
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all repository interfaces and implementations with dependency injection
    /// </summary>
    /// <param name="services">The service collection to register repositories in</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Repositories are registered with Scoped lifetime because they depend on DbContext,
    /// which is also Scoped. This ensures:
    /// 1. One DbContext per HTTP request
    /// 2. All repositories in the same request share the same DbContext
    /// 3. Proper transaction management within a request
    /// 
    /// Registered repositories:
    /// - IEmployeeRepository -> EmployeeRepository
    /// - IDepartmentRepository -> DepartmentRepository
    /// - IDesignationRepository -> DesignationRepository
    /// - IProjectRepository -> ProjectRepository
    /// - ITaskRepository -> TaskRepository
    /// - ILeaveRequestRepository -> LeaveRequestRepository (MongoDB)
    /// - IAuditLogRepository -> AuditLogRepository (MongoDB)
    /// - IReportRepository -> ReportRepository (MongoDB)
    /// </remarks>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // PostgreSQL repositories (using Entity Framework Core)
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        
        // MongoDB repositories (using MongoDB.Driver)
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }

    /// <summary>
    /// Registers all application service interfaces and implementations with dependency injection
    /// </summary>
    /// <param name="services">The service collection to register services in</param>
    /// <returns>The service collection for method chaining</returns>
    /// <remarks>
    /// Services are registered with Scoped lifetime because they depend on repositories,
    /// which are also Scoped. This ensures proper lifecycle management.
    /// 
    /// Services contain business logic and orchestrate:
    /// - Data access via repositories
    /// - Event publishing for audit trail
    /// - Caching operations
    /// - Data transformation (Entity to DTO mapping)
    /// 
    /// Registered services:
    /// - IEmployeeService -> EmployeeService
    /// - IDepartmentService -> DepartmentService
    /// - IDesignationService -> DesignationService
    /// - IProjectService -> ProjectService
    /// - ITaskService -> TaskService
    /// - ILeaveRequestService -> LeaveRequestService
    /// - IAuditLogService -> AuditLogService
    /// - IDashboardService -> DashboardService
    /// </remarks>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    /// <summary>
    /// Configures and initializes database seeding with sample data
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies from</param>
    /// <returns>Task representing the async seeding operation</returns>
    /// <exception cref="Exception">Thrown if database seeding fails</exception>
    /// <remarks>
    /// This method:
    /// 1. Creates a scoped service provider (for accessing scoped services)
    /// 2. Ensures the database is created (for development environments)
    /// 3. Seeds sample data if the database is empty
    /// 
    /// Database seeding is idempotent - it checks if data already exists before seeding.
    /// This allows the method to be called multiple times safely.
    /// 
    /// IMPORTANT: In production, use Entity Framework migrations instead of EnsureCreatedAsync.
    /// EnsureCreatedAsync doesn't support migrations and will drop/recreate the database.
    /// 
    /// The seeding process:
    /// - Seeds PostgreSQL with Departments, Designations, Employees, Projects, ProjectMembers, Tasks
    /// - Seeds MongoDB with LeaveRequests and sample AuditLogs
    /// - Creates realistic relationships between entities
    /// </remarks>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        try
        {
            // Create a scoped service provider to access scoped services (DbContext, DatabaseSeeder)
            // This is necessary because serviceProvider is the root provider, not scoped
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkforceDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

            // Ensure database is created (for development environments)
            // This creates the database and schema if they don't exist
            // NOTE: In production, use migrations: await dbContext.Database.MigrateAsync();
            // EnsureCreatedAsync doesn't support migrations and will drop/recreate the database
            Log.Information("Ensuring database is created...");
            await dbContext.Database.EnsureCreatedAsync();
            Log.Information("Database created/verified successfully.");

            // Seed data (only if database is empty)
            // DatabaseSeeder checks if data exists before seeding to ensure idempotency
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            // Log error and rethrow - database initialization failures should stop application startup
            Log.Error(ex, "Error initializing database");
            throw;
        }
    }
}
