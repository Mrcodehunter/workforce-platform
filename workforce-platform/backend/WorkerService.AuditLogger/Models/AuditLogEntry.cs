using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkerService.AuditLogger.Models;

public class AuditLogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("eventData")]
    public object EventData { get; set; } = new();

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
}
