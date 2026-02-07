using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

public class LeaveRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("employeeId")]
    public Guid EmployeeId { get; set; }

    [BsonElement("employeeName")]
    public string EmployeeName { get; set; } = string.Empty;

    [BsonElement("leaveType")]
    public string LeaveType { get; set; } = string.Empty; // Sick, Casual, Annual, Unpaid

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("approvalHistory")]
    public List<ApprovalHistoryEntry> ApprovalHistory { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ApprovalHistoryEntry
{
    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("changedBy")]
    public string ChangedBy { get; set; } = string.Empty;

    [BsonElement("changedAt")]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("comments")]
    public string? Comments { get; set; }
}
