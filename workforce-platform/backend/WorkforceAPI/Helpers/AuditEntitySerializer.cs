using System.Text.Json;
using WorkforceAPI.Models;
using WorkforceAPI.Models.MongoDB;

namespace WorkforceAPI.Helpers;

/// <summary>
/// Helper class to serialize entities for audit trail without navigation properties
/// Only includes scalar properties and foreign key IDs, excludes all navigation properties
/// </summary>
public static class AuditEntitySerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    /// <summary>
    /// Serialize Employee entity without navigation properties
    /// </summary>
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
    /// Serialize Project entity without navigation properties
    /// </summary>
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
    /// Serialize TaskItem entity without navigation properties
    /// </summary>
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
    /// Serialize Department entity without navigation properties
    /// </summary>
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
    /// Serialize Designation entity without navigation properties
    /// </summary>
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
    /// Serialize LeaveRequest entity (MongoDB model)
    /// </summary>
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
