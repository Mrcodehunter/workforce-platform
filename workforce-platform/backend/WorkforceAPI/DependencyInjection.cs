using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;
using WorkforceAPI.Data;
using WorkforceAPI.EventPublisher;
using WorkforceAPI.Repositories;
using WorkforceAPI.Services;

namespace WorkforceAPI;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all repositories with dependency injection
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }

    /// <summary>
    /// Registers all services with dependency injection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    /// <summary>
    /// Configures and initializes database seeding
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkforceDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

            // Ensure database is created (for development)
            // In production, use migrations: await dbContext.Database.MigrateAsync();
            Log.Information("Ensuring database is created...");
            await dbContext.Database.EnsureCreatedAsync();
            Log.Information("Database created/verified successfully.");

            // Seed data
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing database");
            throw;
        }
    }
}
