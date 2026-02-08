namespace Workforce.Shared.Events;

/// <summary>
/// Enumeration of audit event types used across the workforce platform
/// </summary>
public enum AuditEventType
{
    // Employee Events
    EmployeeCreated,
    EmployeeUpdated,
    EmployeeDeleted,
    
    // Project Events
    ProjectCreated,
    ProjectUpdated,
    ProjectDeleted,
    ProjectMemberAdded,
    ProjectMemberRemoved,
    
    // Task Events
    TaskCreated,
    TaskUpdated,
    TaskDeleted,
    TaskStatusUpdated,
    
    // Leave Request Events
    LeaveRequestCreated,
    LeaveRequestUpdated,
    LeaveRequestApproved,
    LeaveRequestRejected,
    LeaveRequestCancelled,
    
    // Department Events
    DepartmentCreated,
    DepartmentUpdated,
    DepartmentDeleted,
    
    // Designation Events
    DesignationCreated,
    DesignationUpdated,
    DesignationDeleted
}

/// <summary>
/// Helper class to convert AuditEventType enum to string format used in RabbitMQ routing keys
/// </summary>
public static class AuditEventTypeExtensions
{
    public static string ToRoutingKey(this AuditEventType eventType)
    {
        return eventType switch
        {
            AuditEventType.EmployeeCreated => "employee.created",
            AuditEventType.EmployeeUpdated => "employee.updated",
            AuditEventType.EmployeeDeleted => "employee.deleted",
            
            AuditEventType.ProjectCreated => "project.created",
            AuditEventType.ProjectUpdated => "project.updated",
            AuditEventType.ProjectDeleted => "project.deleted",
            AuditEventType.ProjectMemberAdded => "project.member.added",
            AuditEventType.ProjectMemberRemoved => "project.member.removed",
            
            AuditEventType.TaskCreated => "task.created",
            AuditEventType.TaskUpdated => "task.updated",
            AuditEventType.TaskDeleted => "task.deleted",
            AuditEventType.TaskStatusUpdated => "task.status.updated",
            
            AuditEventType.LeaveRequestCreated => "leave.request.created",
            AuditEventType.LeaveRequestUpdated => "leave.request.updated",
            AuditEventType.LeaveRequestApproved => "leave.request.approved",
            AuditEventType.LeaveRequestRejected => "leave.request.rejected",
            AuditEventType.LeaveRequestCancelled => "leave.request.cancelled",
            
            AuditEventType.DepartmentCreated => "department.created",
            AuditEventType.DepartmentUpdated => "department.updated",
            AuditEventType.DepartmentDeleted => "department.deleted",
            
            AuditEventType.DesignationCreated => "designation.created",
            AuditEventType.DesignationUpdated => "designation.updated",
            AuditEventType.DesignationDeleted => "designation.deleted",
            
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unknown event type")
        };
    }
    
    public static string ToEntityType(this AuditEventType eventType)
    {
        return eventType switch
        {
            AuditEventType.EmployeeCreated or 
            AuditEventType.EmployeeUpdated or 
            AuditEventType.EmployeeDeleted => "Employee",
            
            AuditEventType.ProjectCreated or 
            AuditEventType.ProjectUpdated or 
            AuditEventType.ProjectDeleted or
            AuditEventType.ProjectMemberAdded or
            AuditEventType.ProjectMemberRemoved => "Project",
            
            AuditEventType.TaskCreated or 
            AuditEventType.TaskUpdated or 
            AuditEventType.TaskDeleted or 
            AuditEventType.TaskStatusUpdated => "Task",
            
            AuditEventType.LeaveRequestCreated or 
            AuditEventType.LeaveRequestUpdated or 
            AuditEventType.LeaveRequestApproved or 
            AuditEventType.LeaveRequestRejected or 
            AuditEventType.LeaveRequestCancelled => "LeaveRequest",
            
            AuditEventType.DepartmentCreated or 
            AuditEventType.DepartmentUpdated or 
            AuditEventType.DepartmentDeleted => "Department",
            
            AuditEventType.DesignationCreated or 
            AuditEventType.DesignationUpdated or 
            AuditEventType.DesignationDeleted => "Designation",
            
            _ => "Unknown"
        };
    }
}
