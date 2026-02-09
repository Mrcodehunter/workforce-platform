using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

/// <summary>
/// LeaveRequest entity stored in MongoDB
/// </summary>
/// <remarks>
/// This entity represents a leave request submitted by an employee.
/// It is stored in MongoDB (document database) rather than PostgreSQL because:
/// 1. Leave requests have a flexible structure (approval history, varying fields)
/// 2. MongoDB is better suited for document-based data with nested structures
/// 3. Leave requests are typically queried independently (not heavily joined with other entities)
/// 4. Approval history is stored as an embedded array (natural fit for MongoDB)
/// 
/// The entity uses MongoDB BSON attributes to map C# properties to MongoDB document fields.
/// EmployeeId references the Employee entity in PostgreSQL, but EmployeeName is denormalized
/// (stored in MongoDB) for easier querying and display without joins.
/// 
/// Validation rules are defined in LeaveRequestValidator (FluentValidation).
/// </remarks>
public class LeaveRequest
{
    /// <summary>
    /// MongoDB document ID (ObjectId, auto-generated)
    /// </summary>
    /// <remarks>
    /// MongoDB uses ObjectId as the primary key, which is different from PostgreSQL's GUID.
    /// The BsonRepresentation attribute allows using string in C# while storing as ObjectId in MongoDB.
    /// </remarks>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Foreign key to the Employee entity in PostgreSQL (GUID)
    /// </summary>
    /// <remarks>
    /// References the Employee.Id from PostgreSQL.
    /// This maintains referential integrity across databases.
    /// </remarks>
    [BsonElement("employeeId")]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Employee name (denormalized for easier querying)
    /// </summary>
    /// <remarks>
    /// Stored in MongoDB to avoid joins when querying leave requests.
    /// This is denormalized data - if employee name changes, this should be updated.
    /// </remarks>
    [BsonElement("employeeName")]
    public string EmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Type of leave requested (default: empty string)
    /// </summary>
    /// <remarks>
    /// Valid values: "Sick", "Casual", "Annual", "Unpaid"
    /// </remarks>
    [BsonElement("leaveType")]
    public string LeaveType { get; set; } = string.Empty; // Sick, Casual, Annual, Unpaid

    /// <summary>
    /// Start date of the leave period
    /// </summary>
    /// <remarks>
    /// Required. Must be before or equal to EndDate.
    /// </remarks>
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the leave period
    /// </summary>
    /// <remarks>
    /// Required. Must be after or equal to StartDate.
    /// </remarks>
    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Leave request status (default: "Pending")
    /// </summary>
    /// <remarks>
    /// Valid values: "Pending", "Approved", "Rejected", "Cancelled"
    /// New requests default to "Pending" and are updated through the approval workflow.
    /// </remarks>
    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

    /// <summary>
    /// Reason for the leave request (optional, max 1000 characters)
    /// </summary>
    [BsonElement("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// Approval history - list of status changes with timestamps
    /// </summary>
    /// <remarks>
    /// This is an embedded array in MongoDB, storing the complete history of status changes.
    /// Each entry includes: status, who changed it, when, and comments.
    /// This provides a complete audit trail of the approval process.
    /// </remarks>
    [BsonElement("approvalHistory")]
    public List<ApprovalHistoryEntry> ApprovalHistory { get; set; } = new();

    /// <summary>
    /// Timestamp when the leave request was created (UTC)
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the leave request was last updated (UTC)
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Approval history entry for leave requests
/// </summary>
/// <remarks>
/// This class represents a single entry in the approval history of a leave request.
/// It tracks who changed the status, when, and why (comments).
/// 
/// The approval history is stored as an embedded array in the LeaveRequest document,
/// which is a natural fit for MongoDB's document model.
/// </remarks>
public class ApprovalHistoryEntry
{
    /// <summary>
    /// Status at the time of this history entry
    /// </summary>
    /// <remarks>
    /// Examples: "Pending", "Approved", "Rejected", "Cancelled"
    /// </remarks>
    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// User or system that made this status change
    /// </summary>
    /// <remarks>
    /// Typically an email address or username of the approver.
    /// Can be "system" for automatic status changes.
    /// </remarks>
    [BsonElement("changedBy")]
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this status change occurred (UTC)
    /// </summary>
    [BsonElement("changedAt")]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Comments or notes about this status change (optional)
    /// </summary>
    /// <remarks>
    /// Used to provide context for the status change (e.g., "Approved by manager", "Rejected due to policy").
    /// </remarks>
    [BsonElement("comments")]
    public string? Comments { get; set; }
}
