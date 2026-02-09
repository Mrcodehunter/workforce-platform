namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for database connections
/// </summary>
/// <remarks>
/// This class provides a centralized place for database connection strings.
/// Currently, PostgreSQL and MongoDB are used in the workforce platform:
/// - PostgreSQL: Primary relational database for entities (Employee, Project, Task, etc.)
/// - MongoDB: Document database for audit logs and leave requests
/// 
/// Note: This class is defined but not fully integrated into DI yet.
/// Connection strings are currently read directly from configuration.
/// </remarks>
public class DatabaseOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Databases";

    /// <summary>
    /// PostgreSQL connection string
    /// </summary>
    /// <remarks>
    /// Format: "Host=hostname;Port=5432;Database=dbname;Username=user;Password=pass"
    /// 
    /// Used for Entity Framework Core to connect to the primary relational database.
    /// Contains entities like Employee, Project, Task, Department, Designation.
    /// </remarks>
    public string? PostgreSQL { get; set; }

    /// <summary>
    /// MongoDB connection string
    /// </summary>
    /// <remarks>
    /// Format: "mongodb://hostname:27017" or "mongodb://username:password@hostname:27017"
    /// 
    /// Used for MongoDB driver to connect to the document database.
    /// Contains audit logs (AuditLog collection) and leave requests (LeaveRequest collection).
    /// </remarks>
    public string? MongoDB { get; set; }

    /// <summary>
    /// MongoDB database name
    /// </summary>
    /// <remarks>
    /// Default: "workforce_db"
    /// 
    /// The name of the MongoDB database containing audit logs and leave requests.
    /// </remarks>
    public string? MongoDatabaseName { get; set; }
}
