using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using WorkforceAPI.Data;
using WorkforceAPI.Models;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Data;

public class DatabaseSeeder
{
    private readonly WorkforceDbContext _dbContext;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        WorkforceDbContext dbContext,
        IMongoDatabase mongoDatabase,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Check if data already exists
        if (await _dbContext.Employees.AnyAsync())
        {
            _logger.LogInformation("Database already seeded. Skipping...");
            return;
        }

        try
        {
            await SeedPostgreSQLAsync();
            await SeedMongoDBAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private async Task SeedPostgreSQLAsync()
    {
        // Seed Departments
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

    private async Task SeedMongoDBAsync()
    {
        var leaveRequestsCollection = _mongoDatabase.GetCollection<LeaveRequest>("LeaveRequests");
        var random = new Random();

        // Get employee IDs from PostgreSQL
        var employees = await _dbContext.Employees
            .Select(e => new { e.Id, e.FirstName, e.LastName, e.Email })
            .ToListAsync();

        if (!employees.Any())
        {
            _logger.LogWarning("No employees found in PostgreSQL. Skipping MongoDB seeding.");
            return;
        }

        var leaveTypes = new[] { "Sick", "Casual", "Annual", "Unpaid" };
        var statuses = new[] { "Pending", "Approved", "Rejected", "Cancelled" };
        var approvers = new[] { "admin@company.com", "hr@company.com", "manager@company.com" };

        var leaveRequests = new List<LeaveRequest>();

        // Create 2-4 leave requests per employee
        foreach (var employee in employees)
        {
            var requestCount = random.Next(2, 5);
            var employeeName = $"{employee.FirstName} {employee.LastName}";

            for (int i = 0; i < requestCount; i++)
            {
                var startDate = DateTime.UtcNow.AddDays(-random.Next(0, 365));
                var duration = random.Next(1, 10);
                var endDate = startDate.AddDays(duration);
                var leaveType = leaveTypes[random.Next(leaveTypes.Length)];
                var status = statuses[random.Next(statuses.Length)];

                var request = new LeaveRequest
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employeeName,
                    LeaveType = leaveType,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    Reason = $"Leave request for {leaveType.ToLower()} leave",
                    CreatedAt = startDate.AddDays(-random.Next(1, 7)),
                    UpdatedAt = status == "Pending" ? DateTime.UtcNow : startDate.AddDays(random.Next(0, 3))
                };

                // Add approval history if not pending
                if (status != "Pending")
                {
                    request.ApprovalHistory.Add(new ApprovalHistoryEntry
                    {
                        Status = "Pending",
                        ChangedBy = "system",
                        ChangedAt = request.CreatedAt,
                        Comments = "Initial submission"
                    });

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

        if (leaveRequests.Any())
        {
            await leaveRequestsCollection.InsertManyAsync(leaveRequests);
            _logger.LogInformation($"Seeded MongoDB: {leaveRequests.Count} leave requests for {employees.Count} employees");
        }
    }

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
