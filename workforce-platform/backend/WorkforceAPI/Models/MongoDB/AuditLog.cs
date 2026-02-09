using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

/// <summary>
/// AuditLog entity stored in MongoDB
/// </summary>
/// <remarks>
/// This entity represents an audit trail entry for system-wide activity tracking.
/// It is stored in MongoDB (document database) rather than PostgreSQL because:
/// 1. Audit logs have a flexible structure (varying metadata, different entity types)
/// 2. MongoDB is better suited for document-based data with nested structures
/// 3. Audit logs are typically queried independently (not heavily joined with other entities)
/// 4. High write volume (every entity change creates an audit log)
/// 5. Time-series-like queries (query by date ranges, entity types, event types)
/// 
/// The entity stores:
/// - Event information (EventId, EventType, EntityType, EntityId)
/// - Actor information (who/what triggered the event)
/// - Before/after snapshots (entity state before and after changes)
/// - Metadata (flexible dictionary for additional context)
/// 
/// Audit logs are created by the WorkerService.AuditLogger worker service,
/// which consumes events from RabbitMQ and stores them in MongoDB.
/// </remarks>
public class AuditLog
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
    /// Unique event identifier (GUID string)
    /// </summary>
    /// <remarks>
    /// This ID links the audit log to the event published to RabbitMQ.
    /// It is also used to retrieve before/after snapshots from Redis.
    /// </remarks>
    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Event type as routing key string (e.g., "employee.created", "project.updated")
    /// </summary>
    /// <remarks>
    /// This is the RabbitMQ routing key converted from AuditEventType enum.
    /// Examples: "employee.created", "project.member.added", "task.status.updated"
    /// </remarks>
    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty; // employee.created, project.updated, etc.

    /// <summary>
    /// Type of entity that was affected (e.g., "Employee", "Project", "Task")
    /// </summary>
    /// <remarks>
    /// Extracted from AuditEventType using ToEntityType() extension method.
    /// Used for filtering audit logs by entity type.
    /// </remarks>
    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty; // Employee, Project, Task, etc.

    /// <summary>
    /// ID of the entity that was affected (as string for flexibility)
    /// </summary>
    /// <remarks>
    /// Stored as string to accommodate both GUID (PostgreSQL) and ObjectId (MongoDB) formats.
    /// </remarks>
    [BsonElement("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Actor that triggered the event (optional)
    /// </summary>
    /// <remarks>
    /// Typically the user or system that performed the action.
    /// Can be a username, email, or "System" for automated actions.
    /// Currently not fully implemented - placeholder for future authentication integration.
    /// </remarks>
    [BsonElement("actor")]
    public string? Actor { get; set; } // User/System that triggered the event

    /// <summary>
    /// Timestamp when the event occurred (UTC)
    /// </summary>
    /// <remarks>
    /// Default: Current UTC time when the audit log is created.
    /// This is when the event was processed by the audit logger worker, not when it was published.
    /// </remarks>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Snapshot of entity state before the change (optional)
    /// </summary>
    /// <remarks>
    /// This is a JSON string (deserialized as object in MongoDB) containing the entity state
    /// before the change occurred. Retrieved from Redis using key: "audit:{eventId}:before"
    /// 
    /// For create operations, this is typically null (no "before" state exists).
    /// For update/delete operations, this contains the previous entity state.
    /// 
    /// The snapshot is serialized using AuditEntitySerializer to exclude navigation properties.
    /// </remarks>
    [BsonElement("before")]
    public object? Before { get; set; } // Snapshot before change

    /// <summary>
    /// Snapshot of entity state after the change (optional)
    /// </summary>
    /// <remarks>
    /// This is a JSON string (deserialized as object in MongoDB) containing the entity state
    /// after the change occurred. Retrieved from Redis using key: "audit:{eventId}:after"
    /// 
    /// For create operations, this contains the new entity state.
    /// For update operations, this contains the updated entity state.
    /// For delete operations, this is typically null (no "after" state exists).
    /// 
    /// The snapshot is serialized using AuditEntitySerializer to exclude navigation properties.
    /// </remarks>
    [BsonElement("after")]
    public object? After { get; set; } // Snapshot after change

    /// <summary>
    /// Additional metadata for the audit log (flexible dictionary)
    /// </summary>
    /// <remarks>
    /// This dictionary allows storing additional context-specific information.
    /// Examples:
    /// - IP address of the request
    /// - User agent
    /// - Additional context about the change
    /// - Custom fields specific to certain event types
    /// 
    /// The flexible structure allows different event types to store different metadata
    /// without requiring schema changes.
    /// </remarks>
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}
