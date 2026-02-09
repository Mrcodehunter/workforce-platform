using System.Text.Json;
using WorkforceAPI.Models;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Helpers;

/// <summary>
/// Helper class to serialize entities for audit trail without navigation properties
/// </summary>
/// <remarks>
/// This class provides methods to serialize Entity Framework entities to JSON for storage
/// in Redis as audit trail snapshots. It only includes scalar properties and foreign key IDs,
/// excluding all navigation properties to:
/// 1. Avoid circular reference issues during serialization
/// 2. Reduce storage size in Redis
/// 3. Store only essential entity data (not related entities)
/// 4. Prevent serialization of lazy-loaded navigation properties
/// 
/// The serialized data is stored in Redis with keys like:
/// - "audit:{eventId}:before" - Entity state before changes
/// - "audit:{eventId}:after" - Entity state after changes
/// 
/// The audit logger worker retrieves these snapshots and compares them to show what changed.
/// 
/// This approach ensures that:
/// - Navigation properties (Department, Designation, etc.) are not serialized
/// - Only foreign key IDs (DepartmentId, DesignationId) are stored
/// - The serialized data is compact and efficient
/// - No circular reference errors occur
/// </remarks>
public static class AuditEntitySerializer
{
    /// <summary>
    /// JSON serializer options configured for audit trail serialization
    /// </summary>
    /// <remarks>
    /// These options:
    /// - Ignore cycles: Prevents errors when serializing entities with circular references
    /// - WriteIndented = false: Compact JSON (smaller storage size)
    /// </remarks>
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an Employee entity to JSON, excluding navigation properties
    /// </summary>
    /// <param name="employee">The employee entity to serialize</param>
    /// <returns>JSON string containing only scalar properties and foreign key IDs</returns>
    /// <remarks>
    /// This method serializes only the essential employee data:
    /// - Basic information (Id, FirstName, LastName, Email, etc.)
    /// - Foreign key IDs (DepartmentId, DesignationId)
    /// - Scalar properties (Salary, JoiningDate, Skills, etc.)
    /// - Timestamps (CreatedAt, UpdatedAt)
    /// 
    /// Navigation properties (Department, Designation, ProjectMembers, AssignedTasks) are excluded.
    /// </remarks>
    public static string SerializeEmployee(Employee employee)
    {
        var entityData = new
        {
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.IsActive,
            employee.DepartmentId,
            employee.DesignationId,
            employee.Salary,
            employee.JoiningDate,
            employee.Phone,
            employee.Address,
            employee.City,
            employee.Country,
            employee.Skills,
            employee.AvatarUrl,
            employee.CreatedAt,
            employee.UpdatedAt,
            employee.IsDeleted
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }

    /// <summary>
    /// Serializes a Project entity to JSON, excluding navigation properties
    /// </summary>
    /// <param name="project">The project entity to serialize</param>
    /// <returns>JSON string containing only scalar properties</returns>
    /// <remarks>
    /// This method serializes only the essential project data:
    /// - Basic information (Id, Name, Description, Status)
    /// - Dates (StartDate, EndDate)
    /// - Timestamps (CreatedAt, UpdatedAt)
    /// 
    /// Navigation properties (ProjectMembers, Tasks) are excluded.
    /// </remarks>
    public static string SerializeProject(Project project)
    {
        var entityData = new
        {
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.CreatedAt,
            project.UpdatedAt,
            project.IsDeleted
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }

    /// <summary>
    /// Serializes a TaskItem entity to JSON, excluding navigation properties
    /// </summary>
    /// <param name="task">The task entity to serialize</param>
    /// <returns>JSON string containing only scalar properties and foreign key IDs</returns>
    /// <remarks>
    /// This method serializes only the essential task data:
    /// - Basic information (Id, Title, Description, Status, Priority)
    /// - Foreign key IDs (ProjectId, AssignedToEmployeeId)
    /// - Dates (DueDate, CreatedAt, UpdatedAt)
    /// 
    /// Navigation properties (Project, AssignedToEmployee) are excluded.
    /// </remarks>
    public static string SerializeTaskItem(TaskItem task)
    {
        var entityData = new
        {
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.AssignedToEmployeeId,
            task.Priority,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            task.IsDeleted
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }

    /// <summary>
    /// Serializes a Department entity to JSON, excluding navigation properties
    /// </summary>
    /// <param name="department">The department entity to serialize</param>
    /// <returns>JSON string containing only scalar properties</returns>
    /// <remarks>
    /// This method serializes only the essential department data:
    /// - Basic information (Id, Name, Description)
    /// - Timestamps (CreatedAt, UpdatedAt)
    /// 
    /// Navigation properties (if any) are excluded.
    /// </remarks>
    public static string SerializeDepartment(Department department)
    {
        var entityData = new
        {
            department.Id,
            department.Name,
            department.Description,
            department.CreatedAt,
            department.UpdatedAt,
            department.IsDeleted
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }

    /// <summary>
    /// Serializes a Designation entity to JSON, excluding navigation properties
    /// </summary>
    /// <param name="designation">The designation entity to serialize</param>
    /// <returns>JSON string containing only scalar properties</returns>
    /// <remarks>
    /// This method serializes only the essential designation data:
    /// - Basic information (Id, Title, Level, Description)
    /// - Timestamps (CreatedAt, UpdatedAt)
    /// 
    /// Navigation properties (if any) are excluded.
    /// </remarks>
    public static string SerializeDesignation(Designation designation)
    {
        var entityData = new
        {
            designation.Id,
            designation.Title,
            designation.Level,
            designation.Description,
            designation.CreatedAt,
            designation.UpdatedAt
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }

    /// <summary>
    /// Serializes a LeaveRequest entity (MongoDB model) to JSON
    /// </summary>
    /// <param name="leaveRequest">The leave request entity to serialize</param>
    /// <returns>JSON string containing all leave request properties</returns>
    /// <remarks>
    /// This method serializes the complete leave request data:
    /// - Basic information (Id, EmployeeId, EmployeeName, LeaveType, Status)
    /// - Dates (StartDate, EndDate)
    /// - Approval history (complete history of status changes)
    /// - Timestamps (CreatedAt, UpdatedAt)
    /// 
    /// LeaveRequest is a MongoDB document model, so it doesn't have navigation properties
    /// that would cause circular references. All properties are included.
    /// </remarks>
    public static string SerializeLeaveRequest(LeaveRequest leaveRequest)
    {
        var entityData = new
        {
            leaveRequest.Id,
            leaveRequest.EmployeeId,
            leaveRequest.EmployeeName,
            leaveRequest.LeaveType,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.Status,
            leaveRequest.Reason,
            leaveRequest.ApprovalHistory,
            leaveRequest.CreatedAt,
            leaveRequest.UpdatedAt
        };
        return JsonSerializer.Serialize(entityData, SerializerOptions);
    }
}
