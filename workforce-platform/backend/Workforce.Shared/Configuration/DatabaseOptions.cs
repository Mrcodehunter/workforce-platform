namespace Workforce.Shared.Configuration;

/// <summary>
/// Configuration options for databases
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Databases";

    /// <summary>
    /// PostgreSQL connection string
    /// </summary>
    public string? PostgreSQL { get; set; }

    /// <summary>
    /// MongoDB connection string
    /// </summary>
    public string? MongoDB { get; set; }

    /// <summary>
    /// MongoDB database name
    /// </summary>
    public string? MongoDatabaseName { get; set; }
}
