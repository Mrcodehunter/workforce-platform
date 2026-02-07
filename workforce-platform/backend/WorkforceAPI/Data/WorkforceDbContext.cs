using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Models;

namespace WorkforceAPI.Data;

public class WorkforceDbContext : DbContext
{
    public WorkforceDbContext(DbContextOptions<WorkforceDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.DepartmentId);
            e.HasIndex(x => x.DesignationId);
            e.Property(x => x.Skills).HasColumnType("jsonb");
        });

        // Department configuration
        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // Designation configuration
        modelBuilder.Entity<Designation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Title).IsUnique();
        });

        // Project configuration
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Status);
        });

        // ProjectMember many-to-many
        modelBuilder.Entity<ProjectMember>(e =>
        {
            e.HasKey(x => new { x.ProjectId, x.EmployeeId });
            e.HasIndex(x => x.EmployeeId);
        });

        // Task configuration
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProjectId);
            e.HasIndex(x => x.AssignedToEmployeeId);
        });
    }
}
