using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

/// <summary>
/// Report entity stored in MongoDB for cached report data
/// </summary>
/// <remarks>
/// This entity represents a cached report that can be stored in MongoDB for later retrieval.
/// It is stored in MongoDB (document database) rather than PostgreSQL because:
/// 1. Reports have flexible structures (different report types have different data structures)
/// 2. MongoDB is better suited for document-based data with varying schemas
/// 3. Reports are typically queried independently (not joined with other entities)
/// 4. Reports can be large and benefit from MongoDB's flexible document model
/// 
/// The entity uses a flexible "Data" property (object) to accommodate different report types
/// without requiring schema changes. Each report type can have its own data structure.
/// 
/// Currently, this entity is defined but may not be fully utilized in the application.
/// It provides a foundation for future report caching functionality.
/// </remarks>
public class Report
{
    /// <summary>
    /// MongoDB document ID (ObjectId, auto-generated)
    /// </summary>
    /// <remarks>
    /// MongoDB uses ObjectId as the primary key.
    /// The BsonRepresentation attribute allows using string in C# while storing as ObjectId in MongoDB.
    /// </remarks>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Type of report (e.g., "DashboardSummary", "DepartmentHeadcount", "ProjectProgress", "LeaveStatistics")
    /// </summary>
    /// <remarks>
    /// Used to identify the report type and determine how to deserialize the Data property.
    /// Different report types have different data structures.
    /// </remarks>
    [BsonElement("reportType")]
    public string ReportType { get; set; } = string.Empty; // DashboardSummary, DepartmentHeadcount, ProjectProgress, LeaveStatistics

    /// <summary>
    /// Timestamp when the report was generated (UTC)
    /// </summary>
    /// <remarks>
    /// Used to determine if a cached report is still valid (e.g., expire after 1 hour).
    /// </remarks>
    [BsonElement("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Report data (flexible structure based on report type)
    /// </summary>
    /// <remarks>
    /// This property stores the actual report data in a flexible format.
    /// The structure depends on the ReportType:
    /// - "DashboardSummary": DashboardSummaryDto structure
    /// - "DepartmentHeadcount": List of DepartmentHeadcountDto
    /// - "ProjectProgress": List of ProjectProgressDto
    /// - "LeaveStatistics": LeaveStatisticsDto structure
    /// 
    /// The flexible object type allows storing different structures without schema changes.
    /// When retrieving, the Data property should be deserialized based on ReportType.
    /// </remarks>
    [BsonElement("data")]
    public object Data { get; set; } = new(); // Flexible structure based on report type
}
