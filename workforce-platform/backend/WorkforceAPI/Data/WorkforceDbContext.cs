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
            e.Property(x => x.Salary).HasPrecision(18, 2);
            
            // Relationships
            e.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.HasOne(x => x.Designation)
                .WithMany()
                .HasForeignKey(x => x.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.HasMany(x => x.ProjectMembers)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasMany(x => x.AssignedTasks)
                .WithOne(x => x.AssignedToEmployee)
                .HasForeignKey(x => x.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
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
            e.HasIndex(x => x.ProjectId);
            
            // Relationships
            e.HasOne(x => x.Project)
                .WithMany(x => x.ProjectMembers)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(x => x.Employee)
                .WithMany(x => x.ProjectMembers)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Task configuration
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProjectId);
            e.HasIndex(x => x.AssignedToEmployeeId);
            e.HasIndex(x => x.Status);
            
            // Relationships
            e.HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.HasOne(x => x.AssignedToEmployee)
                .WithMany(x => x.AssignedTasks)
                .HasForeignKey(x => x.AssignedToEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
