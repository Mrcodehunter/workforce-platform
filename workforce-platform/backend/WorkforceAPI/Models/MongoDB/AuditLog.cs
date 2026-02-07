using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkforceAPI.Models.MongoDB;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty; // employee.created, project.updated, etc.

    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty; // Employee, Project, Task, etc.

    [BsonElement("entityId")]
    public string EntityId { get; set; } = string.Empty;

    [BsonElement("actor")]
    public string? Actor { get; set; } // User/System that triggered the event

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("before")]
    public object? Before { get; set; } // Snapshot before change

    [BsonElement("after")]
    public object? After { get; set; } // Snapshot after change

    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}
