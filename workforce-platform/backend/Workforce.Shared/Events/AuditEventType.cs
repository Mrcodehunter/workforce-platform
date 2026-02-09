namespace Workforce.Shared.Events;

/// <summary>
/// Enumeration of all audit event types used across the workforce platform
/// </summary>
/// <remarks>
/// This enum provides type-safe event type definitions for the event-driven architecture.
/// Each event type corresponds to a domain operation (create, update, delete, etc.)
/// and is automatically converted to a RabbitMQ routing key using the ToRoutingKey() extension.
/// 
/// Benefits of using an enum instead of raw strings:
/// 1. Compile-time type safety - prevents typos in event type names
/// 2. IntelliSense support - easier to discover available event types
/// 3. Refactoring support - renaming events updates all usages automatically
/// 4. Centralized definition - single source of truth for all event types
/// </remarks>
public enum AuditEventType
{
    // Employee Events
    /// <summary>
    /// Event fired when a new employee is created
    /// </summary>
    EmployeeCreated,
    
    /// <summary>
    /// Event fired when an existing employee is updated
    /// </summary>
    EmployeeUpdated,
    
    /// <summary>
    /// Event fired when an employee is deleted (soft delete)
    /// </summary>
    EmployeeDeleted,
    
    // Project Events
    /// <summary>
    /// Event fired when a new project is created
    /// </summary>
    ProjectCreated,
    
    /// <summary>
    /// Event fired when an existing project is updated
    /// </summary>
    ProjectUpdated,
    
    /// <summary>
    /// Event fired when a project is deleted
    /// </summary>
    ProjectDeleted,
    
    /// <summary>
    /// Event fired when a member is added to a project
    /// </summary>
    ProjectMemberAdded,
    
    /// <summary>
    /// Event fired when a member is removed from a project
    /// </summary>
    ProjectMemberRemoved,
    
    // Task Events
    /// <summary>
    /// Event fired when a new task is created
    /// </summary>
    TaskCreated,
    
    /// <summary>
    /// Event fired when an existing task is updated
    /// </summary>
    TaskUpdated,
    
    /// <summary>
    /// Event fired when a task is deleted
    /// </summary>
    TaskDeleted,
    
    /// <summary>
    /// Event fired when a task's status is updated (e.g., ToDo -> InProgress)
    /// </summary>
    TaskStatusUpdated,
    
    // Leave Request Events
    /// <summary>
    /// Event fired when a new leave request is created
    /// </summary>
    LeaveRequestCreated,
    
    /// <summary>
    /// Event fired when a leave request is updated (general update)
    /// </summary>
    LeaveRequestUpdated,
    
    /// <summary>
    /// Event fired when a leave request is approved
    /// </summary>
    LeaveRequestApproved,
    
    /// <summary>
    /// Event fired when a leave request is rejected
    /// </summary>
    LeaveRequestRejected,
    
    /// <summary>
    /// Event fired when a leave request is cancelled
    /// </summary>
    LeaveRequestCancelled,
    
    // Department Events
    /// <summary>
    /// Event fired when a new department is created
    /// </summary>
    DepartmentCreated,
    
    /// <summary>
    /// Event fired when an existing department is updated
    /// </summary>
    DepartmentUpdated,
    
    /// <summary>
    /// Event fired when a department is deleted
    /// </summary>
    DepartmentDeleted,
    
    // Designation Events
    /// <summary>
    /// Event fired when a new designation is created
    /// </summary>
    DesignationCreated,
    
    /// <summary>
    /// Event fired when an existing designation is updated
    /// </summary>
    DesignationUpdated,
    
    /// <summary>
    /// Event fired when a designation is deleted
    /// </summary>
    DesignationDeleted
}

/// <summary>
/// Extension methods for AuditEventType enum
/// Provides conversion utilities for RabbitMQ routing keys and entity type identification
/// </summary>
/// <remarks>
/// These extensions bridge the gap between the type-safe enum and the string-based
/// RabbitMQ routing system. They ensure consistent routing key formatting across the application.
/// </remarks>
public static class AuditEventTypeExtensions
{
    /// <summary>
    /// Converts an AuditEventType enum value to its corresponding RabbitMQ routing key string
    /// </summary>
    /// <param name="eventType">The audit event type enum value</param>
    /// <returns>The routing key string (e.g., "employee.created", "project.member.added")</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if eventType is not recognized</exception>
    /// <remarks>
    /// Routing keys follow the pattern: "{entity}.{action}" or "{entity}.{subentity}.{action}"
    /// Examples:
    /// - EmployeeCreated -> "employee.created"
    /// - ProjectMemberAdded -> "project.member.added"
    /// - TaskStatusUpdated -> "task.status.updated"
    /// 
    /// These routing keys allow workers to subscribe to event patterns using wildcards:
    /// - "employee.*" - all employee events
    /// - "project.member.*" - all project member events
    /// - "#" - all events
    /// </remarks>
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
    
    /// <summary>
    /// Extracts the entity type name from an AuditEventType
    /// </summary>
    /// <param name="eventType">The audit event type enum value</param>
    /// <returns>The entity type name (e.g., "Employee", "Project", "Task")</returns>
    /// <remarks>
    /// This is used by the audit logger worker to categorize audit logs by entity type.
    /// For composite events like ProjectMemberAdded, it still returns "Project" since
    /// the event is about a project's members, not a separate "ProjectMember" entity.
    /// </remarks>
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
