using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;

namespace WorkforceAPI.Data;

/// <summary>
/// Entity Framework Core DbContext for the Workforce Management system
/// </summary>
/// <remarks>
/// This DbContext represents the PostgreSQL database schema and provides access to all
/// relational entities in the system. It uses Entity Framework Core's Code-First approach,
/// where the database schema is defined in C# code rather than SQL scripts.
/// 
/// Key features:
/// - Manages database connections and transactions
/// - Tracks entity changes for automatic updates
/// - Provides LINQ queries that are translated to SQL
/// - Handles relationship mapping and foreign keys
/// - Configures indexes, constraints, and data types
/// 
/// Lifetime: Scoped (one instance per HTTP request)
/// This ensures:
/// - Each request has its own database context
/// - Changes are isolated per request
/// - Proper transaction management
/// - Automatic disposal at end of request
/// </remarks>
public class WorkforceDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of WorkforceDbContext
    /// </summary>
    /// <param name="options">DbContext options including connection string and provider</param>
    /// <remarks>
    /// The options are configured in Program.cs using AddDbContext.
    /// They include the PostgreSQL connection string and Npgsql provider configuration.
    /// </remarks>
    public WorkforceDbContext(DbContextOptions<WorkforceDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Employees table - stores all employee information
    /// </summary>
    public DbSet<Employee> Employees { get; set; }
    
    /// <summary>
    /// Departments table - stores organizational departments
    /// </summary>
    public DbSet<Department> Departments { get; set; }
    
    /// <summary>
    /// Designations table - stores job titles and levels
    /// </summary>
    public DbSet<Designation> Designations { get; set; }
    
    /// <summary>
    /// Projects table - stores project information
    /// </summary>
    public DbSet<Project> Projects { get; set; }
    
    /// <summary>
    /// ProjectMembers table - many-to-many relationship between Projects and Employees
    /// </summary>
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    
    /// <summary>
    /// Tasks table - stores task items assigned to projects and employees
    /// </summary>
    public DbSet<TaskItem> Tasks { get; set; }

    /// <summary>
    /// Configures the database model using Fluent API
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entities</param>
    /// <remarks>
    /// This method is called by Entity Framework Core during model creation.
    /// It configures:
    /// - Primary keys and indexes
    /// - Foreign key relationships
    /// - Delete behaviors (Cascade, Restrict, SetNull)
    /// - Data types and constraints
    /// - Unique constraints
    /// 
    /// Using Fluent API instead of data annotations provides:
    /// - More flexibility
    /// - Better separation of concerns (configuration separate from entity classes)
    /// - Ability to configure relationships that can't be expressed with attributes
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(e =>
        {
            // Primary key
            e.HasKey(x => x.Id);
            
            // Unique index on Email to prevent duplicate email addresses
            e.HasIndex(x => x.Email).IsUnique();
            
            // Indexes on foreign keys for faster joins
            e.HasIndex(x => x.DepartmentId);
            e.HasIndex(x => x.DesignationId);
            
            // Store Skills array as PostgreSQL JSONB type
            // JSONB provides better performance and querying capabilities than JSON
            e.Property(x => x.Skills).HasColumnType("jsonb");
            
            // Salary precision: 18 total digits, 2 decimal places
            // Supports values up to 999,999,999,999,999,999.99
            e.Property(x => x.Salary).HasPrecision(18, 2);
            
            // Relationships
            // Employee -> Department (many-to-one)
            // Restrict delete: Cannot delete a department if employees are assigned to it
            e.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Employee -> Designation (many-to-one)
            // Restrict delete: Cannot delete a designation if employees have it
            e.HasOne(x => x.Designation)
                .WithMany()
                .HasForeignKey(x => x.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Employee -> ProjectMembers (one-to-many)
            // Cascade delete: When employee is deleted, remove all project memberships
            e.HasMany(x => x.ProjectMembers)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Employee -> AssignedTasks (one-to-many)
            // SetNull delete: When employee is deleted, set AssignedToEmployeeId to null (tasks remain)
            e.HasMany(x => x.AssignedTasks)
                .WithOne(x => x.AssignedToEmployee)
                .HasForeignKey(x => x.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Department configuration
        modelBuilder.Entity<Department>(e =>
        {
            // Primary key
            e.HasKey(x => x.Id);
            
            // Unique index on Name to prevent duplicate department names
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Designation configuration
        modelBuilder.Entity<Designation>(e =>
        {
            // Primary key
            e.HasKey(x => x.Id);
            
            // Unique index on Title to prevent duplicate designation titles
            e.HasIndex(x => x.Title).IsUnique();
        });

        // Project configuration
        modelBuilder.Entity<Project>(e =>
        {
            // Primary key
            e.HasKey(x => x.Id);
            
            // Index on Status for faster filtering by project status
            // Common queries: "Get all active projects", "Get all completed projects"
            e.HasIndex(x => x.Status);
        });

        // ProjectMember configuration (many-to-many relationship table)
        // This table represents the many-to-many relationship between Projects and Employees
        modelBuilder.Entity<ProjectMember>(e =>
        {
            // Composite primary key: (ProjectId, EmployeeId)
            // Ensures an employee can only be a member of a project once
            e.HasKey(x => new { x.ProjectId, x.EmployeeId });
            
            // Indexes on foreign keys for faster joins
            e.HasIndex(x => x.EmployeeId);
            e.HasIndex(x => x.ProjectId);
            
            // Relationships
            // ProjectMember -> Project (many-to-one)
            // Cascade delete: When project is deleted, remove all project memberships
            e.HasOne(x => x.Project)
                .WithMany(x => x.ProjectMembers)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // ProjectMember -> Employee (many-to-one)
            // Cascade delete: When employee is deleted, remove all project memberships
            e.HasOne(x => x.Employee)
                .WithMany(x => x.ProjectMembers)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Task configuration
        modelBuilder.Entity<TaskItem>(e =>
        {
            // Primary key
            e.HasKey(x => x.Id);
            
            // Indexes on foreign keys and commonly queried fields
            e.HasIndex(x => x.ProjectId);  // For "Get all tasks for a project"
            e.HasIndex(x => x.AssignedToEmployeeId);  // For "Get all tasks for an employee"
            e.HasIndex(x => x.Status);  // For "Get all tasks with status X"
            
            // Relationships
            // Task -> Project (many-to-one)
            // Cascade delete: When project is deleted, remove all tasks
            e.HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Task -> AssignedToEmployee (many-to-one, nullable)
            // SetNull delete: When employee is deleted, set AssignedToEmployeeId to null (tasks remain unassigned)
            e.HasOne(x => x.AssignedToEmployee)
                .WithMany(x => x.AssignedTasks)
                .HasForeignKey(x => x.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
