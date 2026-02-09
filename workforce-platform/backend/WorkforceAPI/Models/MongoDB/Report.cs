using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

public class Report
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("reportType")]
    public string ReportType { get; set; } = string.Empty; // DashboardSummary, DepartmentHeadcount, ProjectProgress, LeaveStatistics

    [BsonElement("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("data")]
    public object Data { get; set; } = new(); // Flexible structure based on report type
}
