using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WorkforceAPI.Data;
using WorkforceAPI.Models;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Data;

/// <summary>
/// Database seeder for populating the database with sample data
/// </summary>
/// <remarks>
/// This class is responsible for seeding both PostgreSQL and MongoDB databases with
/// realistic sample data for development and testing purposes. The seeding process is
/// idempotent - it checks if data already exists before seeding to prevent duplicate data.
/// 
/// Seeded data includes:
/// - PostgreSQL: Departments, Designations, Employees (55+), Projects (8), ProjectMembers, Tasks
/// - MongoDB: LeaveRequests (2-4 per employee), sample AuditLogs
/// 
/// The seeder creates realistic relationships between entities:
/// - Employees are assigned to Departments and Designations
/// - Employees are assigned to Projects with roles
/// - Tasks are created for Projects and assigned to Employees
/// - LeaveRequests reference Employees
/// 
/// This is primarily used for:
/// - Local development (quick setup with sample data)
/// - Testing (consistent test data)
/// - Demos (realistic data for presentations)
/// 
/// In production, this should be disabled or replaced with proper data migration scripts.
/// </remarks>
public class DatabaseSeeder
{
    private readonly WorkforceDbContext _dbContext;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DatabaseSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of DatabaseSeeder
    /// </summary>
    /// <param name="dbContext">PostgreSQL database context</param>
    /// <param name="mongoDatabase">MongoDB database instance</param>
    /// <param name="logger">Logger for seeding progress and errors</param>
    public DatabaseSeeder(
        WorkforceDbContext dbContext,
        IMongoDatabase mongoDatabase,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    /// <summary>
    /// Seeds both PostgreSQL and MongoDB databases with sample data
    /// </summary>
    /// <returns>Task representing the async seeding operation</returns>
    /// <exception cref="Exception">Thrown if seeding fails</exception>
    /// <remarks>
    /// This method is idempotent - it checks if data already exists before seeding.
    /// If employees already exist in the database, seeding is skipped to prevent duplicate data.
    /// 
    /// The seeding process:
    /// 1. Checks if data already exists (idempotency check)
    /// 2. Seeds PostgreSQL with relational data
    /// 3. Seeds MongoDB with document data
    /// 4. Logs progress and completion
    /// 
    /// This method is called once at application startup from Program.cs.
    /// </remarks>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Check if data already exists (idempotency check)
        // If employees exist, assume database is already seeded
        // This prevents duplicate data on multiple application restarts
        if (await _dbContext.Employees.AnyAsync())
        {
            _logger.LogInformation("Database already seeded. Skipping...");
            return;
        }

        try
        {
            // Seed PostgreSQL first (Departments, Designations, Employees, Projects, Tasks)
            // MongoDB seeding depends on employee IDs from PostgreSQL
            await SeedPostgreSQLAsync();
            
            // Seed MongoDB (LeaveRequests, AuditLogs)
            await SeedMongoDBAsync();
            
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            // Log error and rethrow - seeding failures should stop application startup
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    /// <summary>
    /// Seeds PostgreSQL database with sample relational data
    /// </summary>
    /// <returns>Task representing the async seeding operation</returns>
    /// <remarks>
    /// This method seeds the following entities in order:
    /// 1. Departments (8 departments: Engineering, Product, Sales, etc.)
    /// 2. Designations (10 designations: Junior Developer, Senior Developer, Manager, etc.)
    /// 3. Employees (55 employees with random assignments to departments and designations)
    /// 4. Projects (8 projects with various statuses)
    /// 5. ProjectMembers (many-to-many: 3-8 members per project)
    /// 6. Tasks (5-15 tasks per active project, assigned to project members)
    /// 
    /// The seeding creates realistic relationships:
    /// - Employees are randomly assigned to Departments and Designations
    /// - Employees are randomly assigned to Projects with roles (Developer, Lead, QA, etc.)
    /// - Tasks are created for active projects and assigned to project members
    /// - Dates are randomized within realistic ranges
    /// 
    /// All entities are saved in batches for better performance.
    /// </remarks>
    private async Task SeedPostgreSQLAsync()
    {
        // Seed Departments - Organizational units
        // These are created first as they are referenced by Employees
        var departments = new List<Department>
        {
            new() { Id = Guid.NewGuid(), Name = "Engineering", Description = "Software development and technical operations" },
            new() { Id = Guid.NewGuid(), Name = "Product", Description = "Product management and strategy" },
            new() { Id = Guid.NewGuid(), Name = "Sales", Description = "Sales and business development" },
            new() { Id = Guid.NewGuid(), Name = "Marketing", Description = "Marketing and communications" },
            new() { Id = Guid.NewGuid(), Name = "HR", Description = "Human resources and talent management" },
            new() { Id = Guid.NewGuid(), Name = "Finance", Description = "Finance and accounting" },
            new() { Id = Guid.NewGuid(), Name = "Operations", Description = "Operations and administration" },
            new() { Id = Guid.NewGuid(), Name = "Support", Description = "Customer support and success" }
        };
        await _dbContext.Departments.AddRangeAsync(departments);
        await _dbContext.SaveChangesAsync();

        // Seed Designations
        var designations = new List<Designation>
        {
            new() { Id = Guid.NewGuid(), Title = "Junior Developer", Level = 1, Description = "Entry-level software developer" },
            new() { Id = Guid.NewGuid(), Title = "Software Developer", Level = 2, Description = "Mid-level software developer" },
            new() { Id = Guid.NewGuid(), Title = "Senior Developer", Level = 3, Description = "Senior software developer" },
            new() { Id = Guid.NewGuid(), Title = "Tech Lead", Level = 4, Description = "Technical team lead" },
            new() { Id = Guid.NewGuid(), Title = "Engineering Manager", Level = 5, Description = "Engineering department manager" },
            new() { Id = Guid.NewGuid(), Title = "Product Manager", Level = 3, Description = "Product management role" },
            new() { Id = Guid.NewGuid(), Title = "Sales Representative", Level = 2, Description = "Sales team member" },
            new() { Id = Guid.NewGuid(), Title = "Sales Manager", Level = 4, Description = "Sales team manager" },
            new() { Id = Guid.NewGuid(), Title = "HR Specialist", Level = 2, Description = "Human resources specialist" },
            new() { Id = Guid.NewGuid(), Title = "HR Manager", Level = 4, Description = "Human resources manager" }
        };
        await _dbContext.Designations.AddRangeAsync(designations);
        await _dbContext.SaveChangesAsync();

        // Seed Employees (50+ employees)
        var random = new Random();
        var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emily", "Robert", "Jessica", "William", "Ashley", "James", "Amanda", "Christopher", "Melissa", "Daniel", "Michelle", "Matthew", "Kimberly", "Anthony", "Amy", "Mark", "Angela", "Donald", "Lisa", "Steven", "Nancy", "Paul", "Karen", "Andrew", "Betty", "Joshua", "Helen", "Kenneth", "Sandra", "Kevin", "Donna", "Brian", "Carol", "George", "Ruth", "Edward", "Sharon", "Ronald", "Michelle", "Timothy", "Laura", "Jason", "Sarah", "Jeffrey", "Kimberly" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Gomez", "Phillips" };
        var skills = new[] { "C#", "React", "TypeScript", "PostgreSQL", "MongoDB", "Docker", "RabbitMQ", "Node.js", "Python", "Java", "JavaScript", "SQL", "Entity Framework", "ASP.NET", "REST API", "Microservices", "CI/CD", "Git", "Azure", "AWS" };

        var employees = new List<Employee>();
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose" };
        var countries = new[] { "USA", "Canada", "UK", "Australia" };

        for (int i = 0; i < 55; i++)
        {
            var dept = departments[random.Next(departments.Count)];
            var desig = designations[random.Next(designations.Count)];
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@company.com";
            
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsActive = random.Next(10) > 0, // 90% active
                DepartmentId = dept.Id,
                DesignationId = desig.Id,
                Salary = random.Next(50000, 150000),
                JoiningDate = DateTime.UtcNow.AddDays(-random.Next(365, 2000)),
                Phone = $"+1-{random.Next(200, 999)}-{random.Next(200, 999)}-{random.Next(1000, 9999)}",
                Address = $"{random.Next(100, 9999)} Main St",
                City = cities[random.Next(cities.Length)],
                Country = countries[random.Next(countries.Length)],
                Skills = skills.OrderBy(x => random.Next()).Take(random.Next(3, 8)).ToList(),
                AvatarUrl = $"https://api.dicebear.com/7.x/avataaars/svg?seed={firstName}{lastName}",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365, 2000)),
                UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
            };
            employees.Add(employee);
        }
        await _dbContext.Employees.AddRangeAsync(employees);
        await _dbContext.SaveChangesAsync();

        // Seed Projects
        var projects = new List<Project>
        {
            new() { Id = Guid.NewGuid(), Name = "Customer Portal Redesign", Description = "Modernize customer-facing portal", Status = "Active", StartDate = DateTime.UtcNow.AddMonths(-6), EndDate = DateTime.UtcNow.AddMonths(3) },
            new() { Id = Guid.NewGuid(), Name = "Mobile App Development", Description = "Native mobile application", Status = "Active", StartDate = DateTime.UtcNow.AddMonths(-4), EndDate = DateTime.UtcNow.AddMonths(8) },
            new() { Id = Guid.NewGuid(), Name = "API Gateway Migration", Description = "Migrate to new API gateway", Status = "Active", StartDate = DateTime.UtcNow.AddMonths(-2), EndDate = DateTime.UtcNow.AddMonths(4) },
            new() { Id = Guid.NewGuid(), Name = "Data Analytics Platform", Description = "Build analytics and reporting platform", Status = "Planning", StartDate = DateTime.UtcNow.AddMonths(1), EndDate = null },
            new() { Id = Guid.NewGuid(), Name = "Security Audit System", Description = "Implement security audit logging", Status = "Active", StartDate = DateTime.UtcNow.AddMonths(-3), EndDate = DateTime.UtcNow.AddMonths(2) },
            new() { Id = Guid.NewGuid(), Name = "Legacy System Migration", Description = "Migrate legacy systems to cloud", Status = "OnHold", StartDate = DateTime.UtcNow.AddMonths(-12), EndDate = null },
            new() { Id = Guid.NewGuid(), Name = "E-commerce Integration", Description = "Integrate with e-commerce platforms", Status = "Completed", StartDate = DateTime.UtcNow.AddMonths(-18), EndDate = DateTime.UtcNow.AddMonths(-6) },
            new() { Id = Guid.NewGuid(), Name = "Performance Optimization", Description = "Optimize application performance", Status = "Active", StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(5) }
        };
        await _dbContext.Projects.AddRangeAsync(projects);
        await _dbContext.SaveChangesAsync();

        // Seed Project Members (many-to-many)
        var projectMembers = new List<ProjectMember>();
        var roles = new[] { "Developer", "Lead Developer", "QA Engineer", "Project Manager", "Tech Lead", "Architect" };
        
        foreach (var project in projects)
        {
            var memberCount = random.Next(3, 8);
            var selectedEmployees = employees.OrderBy(x => random.Next()).Take(memberCount).ToList();
            
            foreach (var employee in selectedEmployees)
            {
                projectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    EmployeeId = employee.Id,
                    Role = roles[random.Next(roles.Length)],
                    JoinedAt = project.StartDate.AddDays(random.Next(0, 30))
                });
            }
        }
        await _dbContext.ProjectMembers.AddRangeAsync(projectMembers);
        await _dbContext.SaveChangesAsync();

        // Seed Tasks
        var tasks = new List<TaskItem>();
        var taskStatuses = new[] { "ToDo", "InProgress", "InReview", "Done", "Cancelled" };
        var priorities = new[] { 0, 1, 2, 3 }; // Low, Medium, High, Critical

        foreach (var project in projects.Where(p => p.Status != "Planning"))
        {
            var taskCount = random.Next(5, 15);
            var projectEmployees = projectMembers
                .Where(pm => pm.ProjectId == project.Id)
                .Select(pm => pm.EmployeeId)
                .ToList();

            for (int i = 0; i < taskCount; i++)
            {
                var assignedEmployee = projectEmployees.Any() 
                    ? projectEmployees[random.Next(projectEmployees.Count)] 
                    : (Guid?)null;

                tasks.Add(new TaskItem
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Title = $"Task {i + 1}: {GetTaskTitle()}",
                    Description = $"Description for task {i + 1} in {project.Name}",
                    Status = taskStatuses[random.Next(taskStatuses.Length)],
                    AssignedToEmployeeId = assignedEmployee,
                    Priority = priorities[random.Next(priorities.Length)],
                    DueDate = project.EndDate?.AddDays(-random.Next(0, 30)) ?? DateTime.UtcNow.AddDays(random.Next(7, 60)),
                    CreatedAt = project.StartDate.AddDays(random.Next(0, (DateTime.UtcNow - project.StartDate).Days)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 7))
                });
            }
        }
        await _dbContext.Tasks.AddRangeAsync(tasks);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"Seeded PostgreSQL: {departments.Count} departments, {designations.Count} designations, {employees.Count} employees, {projects.Count} projects, {projectMembers.Count} project members, {tasks.Count} tasks");
    }

    /// <summary>
    /// Seeds MongoDB database with sample document data
    /// </summary>
    /// <returns>Task representing the async seeding operation</returns>
    /// <remarks>
    /// This method seeds MongoDB with LeaveRequest documents. It depends on employees
    /// being seeded in PostgreSQL first, as leave requests reference employee IDs.
    /// 
    /// The seeding process:
    /// 1. Retrieves employee IDs from PostgreSQL
    /// 2. Creates 2-4 leave requests per employee
    /// 3. Randomly assigns leave types (Sick, Casual, Annual, Unpaid)
    /// 4. Randomly assigns statuses (Pending, Approved, Rejected, Cancelled)
    /// 5. Creates approval history entries for non-pending requests
    /// 6. Inserts all leave requests in a single batch operation
    /// 
    /// Leave requests are created with:
    /// - Random start dates (within the past year)
    /// - Random durations (1-10 days)
    /// - Realistic approval history (for non-pending requests)
    /// - Proper timestamps (CreatedAt before UpdatedAt)
    /// </remarks>
    private async Task SeedMongoDBAsync()
    {
        // Get MongoDB collection for LeaveRequests
        var leaveRequestsCollection = _mongoDatabase.GetCollection<LeaveRequest>("LeaveRequests");
        var random = new Random();

        // Get employee IDs from PostgreSQL
        // Only retrieve essential fields to minimize data transfer
        var employees = await _dbContext.Employees
            .Select(e => new { e.Id, e.FirstName, e.LastName, e.Email })
            .ToListAsync();

        // If no employees exist, skip MongoDB seeding
        // This ensures data consistency (leave requests require employees)
        if (!employees.Any())
        {
            _logger.LogWarning("No employees found in PostgreSQL. Skipping MongoDB seeding.");
            return;
        }

        // Define possible values for leave requests
        var leaveTypes = new[] { "Sick", "Casual", "Annual", "Unpaid" };
        var statuses = new[] { "Pending", "Approved", "Rejected", "Cancelled" };
        var approvers = new[] { "admin@company.com", "hr@company.com", "manager@company.com" };

        var leaveRequests = new List<LeaveRequest>();

        // Create 2-4 leave requests per employee
        // This creates a realistic distribution of leave requests
        foreach (var employee in employees)
        {
            var requestCount = random.Next(2, 5);  // 2, 3, or 4 requests per employee
            var employeeName = $"{employee.FirstName} {employee.LastName}";

            for (int i = 0; i < requestCount; i++)
            {
                // Generate random dates within the past year
                var startDate = DateTime.UtcNow.AddDays(-random.Next(0, 365));
                var duration = random.Next(1, 10);  // 1-10 days
                var endDate = startDate.AddDays(duration);
                var leaveType = leaveTypes[random.Next(leaveTypes.Length)];
                var status = statuses[random.Next(statuses.Length)];

                // Create leave request with realistic data
                var request = new LeaveRequest
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employeeName,
                    LeaveType = leaveType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    Reason = $"Leave request for {leaveType.ToLower()} leave",
                    // CreatedAt is before start date (request submitted in advance)
                    CreatedAt = startDate.AddDays(-random.Next(1, 7)),
                    // UpdatedAt depends on status (pending requests updated recently)
                    UpdatedAt = status == "Pending" ? DateTime.UtcNow : startDate.AddDays(random.Next(0, 3))
                };

                // Add approval history based on status
                // Non-pending requests have a complete approval history
                if (status != "Pending")
                {
                    // Initial submission entry
                    request.ApprovalHistory.Add(new ApprovalHistoryEntry
                    {
                        Status = "Pending",
                        ChangedBy = "system",
                        ChangedAt = request.CreatedAt,
                        Comments = "Initial submission"
                    });

                    // Approval/rejection entry
                    request.ApprovalHistory.Add(new ApprovalHistoryEntry
                    {
                        Status = status,
                        ChangedBy = approvers[random.Next(approvers.Length)],
                        ChangedAt = request.UpdatedAt,
                        Comments = status == "Approved" ? "Approved by manager" : "Request rejected due to policy"
                    });
                }
                else
                {
                    // Pending requests only have initial submission entry
                    request.ApprovalHistory.Add(new ApprovalHistoryEntry
                    {
                        Status = "Pending",
                        ChangedBy = "system",
                        ChangedAt = request.CreatedAt,
                        Comments = "Awaiting approval"
                    });
                }

                leaveRequests.Add(request);
            }
        }

        // Insert all leave requests in a single batch operation for better performance
        if (leaveRequests.Any())
        {
            await leaveRequestsCollection.InsertManyAsync(leaveRequests);
            _logger.LogInformation($"Seeded MongoDB: {leaveRequests.Count} leave requests for {employees.Count} employees");
        }
    }

    /// <summary>
    /// Generates a random task title from a predefined list
    /// </summary>
    /// <returns>A random task title</returns>
    /// <remarks>
    /// This helper method provides realistic task titles for seeded data.
    /// The titles represent common software development tasks.
    /// </remarks>
    private string GetTaskTitle()
    {
        var titles = new[]
        {
            "Implement user authentication",
            "Design database schema",
            "Write unit tests",
            "Fix bug in payment processing",
            "Optimize database queries",
            "Add logging functionality",
            "Create API documentation",
            "Implement caching layer",
            "Refactor legacy code",
            "Add error handling",
            "Create admin dashboard",
            "Implement search functionality",
            "Add data validation",
            "Create migration scripts",
            "Implement file upload",
            "Add notification system",
            "Create report generator",
            "Implement audit logging",
            "Add monitoring and alerts",
            "Create backup system"
        };
        return titles[new Random().Next(titles.Length)];
    }
}
