using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Workforce.Shared.Cache;

namespace WorkerService.AuditLogger.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ILogger<AuditLogService> _logger;
    private readonly IRedisCache _redisCache;
    private IMongoCollection<Models.AuditLog>? _auditCollection;

    public AuditLogService(IMongoDatabase mongoDatabase, ILogger<AuditLogService> logger, IRedisCache redisCache)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
        _redisCache = redisCache;
    }

    private IMongoCollection<Models.AuditLog> AuditCollection
    {
        get
        {
            if (_auditCollection == null)
            {
                _auditCollection = _mongoDatabase.GetCollection<Models.AuditLog>("AuditLogs");
            }
            return _auditCollection;
        }
    }

    public async Task LogEventAsync(string eventId, string eventType, object eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if event already processed (idempotency)
            var existing = await AuditCollection
                .Find(a => a.EventId == eventId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null)
            {
                _logger.LogInformation("Event {EventId} already processed, skipping", eventId);
                return;
            }

            // Extract entity information from event type and data
            var (entityType, entityId) = ExtractEntityInfo(eventType, eventData);
            
            // Read before/after snapshots from Redis
            object? before = null;
            object? after = null;
            var metadata = new Dictionary<string, object>();

            try
            {
                // Try to get before snapshot from Redis
                var beforeJson = await _redisCache.GetAsync<string>($"audit:{eventId}:before");
                if (!string.IsNullOrEmpty(beforeJson))
                {
                    before = ConvertToBsonCompatible(JsonSerializer.Deserialize<object>(beforeJson)!);
                }

                // Try to get after snapshot from Redis
                var afterJson = await _redisCache.GetAsync<string>($"audit:{eventId}:after");
                if (!string.IsNullOrEmpty(afterJson))
                {
                    after = ConvertToBsonCompatible(JsonSerializer.Deserialize<object>(afterJson)!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading before/after snapshots from Redis for event {EventId}", eventId);
            }

            // Convert eventData to MongoDB-serializable format as fallback
            object? serializableEventData = ConvertToBsonCompatible(eventData);

            var auditLog = new Models.AuditLog
            {
                EventId = eventId,
                EventType = eventType,
                EntityType = entityType,
                EntityId = entityId,
                Actor = "System", // TODO: Extract from event data or context
                Timestamp = DateTime.UtcNow,
                Before = before,
                After = after ?? serializableEventData, // Use converted event data as "after" if not explicitly provided
                Metadata = metadata
            };

            await AuditCollection.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
            _logger.LogInformation("Audit log created for event {EventId}, entity: {EntityType}/{EntityId}", eventId, entityType, entityId);

            // Clean up Redis keys after successful audit log creation
            try
            {
                await _redisCache.DeleteAsync($"audit:{eventId}:before");
                await _redisCache.DeleteAsync($"audit:{eventId}:after");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up Redis keys for event {EventId}", eventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for event {EventId}", eventId);
            throw;
        }
    }

    private (string entityType, string entityId) ExtractEntityInfo(string eventType, object eventData)
    {
        // Extract entity type from event type (e.g., "employee.created" -> "Employee")
        var entityType = ExtractEntityTypeFromEventType(eventType);
        
        // Extract entity ID from event data
        var entityId = ExtractEntityIdFromEventData(eventData, entityType);

        return (entityType, entityId);
    }

    private string ExtractEntityTypeFromEventType(string eventType)
    {
        // Event types: employee.created, project.updated, task.deleted, leave.request.approved, project.member.added
        var parts = eventType.Split('.');
        if (parts.Length > 0)
        {
            var firstPart = parts[0];
            
            // Handle special cases
            if (firstPart == "leave" && parts.Length > 1 && parts[1] == "request")
            {
                return "LeaveRequest";
            }
            
            // For project.member.* events, return "Project" as entity type
            if (firstPart == "project" && parts.Length > 1 && parts[1] == "member")
            {
                return "Project";
            }
            
            // Capitalize first letter
            return char.ToUpperInvariant(firstPart[0]) + firstPart.Substring(1);
        }
        return "Unknown";
    }

    private string ExtractEntityIdFromEventData(object eventData, string entityType)
    {
        try
        {
            // Event payload structure: { EventId, EventType, Timestamp, Data: { EmployeeId, ProjectId, etc. } }
            
            if (eventData is JsonElement jsonElement)
            {
                // First try to get from Data property
                if (jsonElement.TryGetProperty("Data", out var dataElement))
                {
                    // Try common ID property names
                    var idPropertyNames = new[]
                    {
                        $"{entityType}Id",
                        $"{entityType.ToLowerInvariant()}Id",
                        "Id",
                        "id"
                    };

                    foreach (var propName in idPropertyNames)
                    {
                        if (dataElement.TryGetProperty(propName, out var idElement))
                        {
                            return idElement.ToString();
                        }
                    }
                }
            }
            else if (eventData is Dictionary<string, object> eventDict)
            {
                // First try to get from Data property
                if (eventDict.TryGetValue("Data", out var dataObj))
                {
                    Dictionary<string, object>? dataDict = null;
                    
                    // Handle JsonElement wrapped in object
                    if (dataObj is JsonElement dataJsonElement)
                    {
                        var converted = ConvertJsonElement(dataJsonElement);
                        dataDict = converted as Dictionary<string, object>;
                    }
                    else if (dataObj is Dictionary<string, object> dict)
                    {
                        dataDict = dict;
                    }
                    else
                    {
                        // Try to convert to dictionary
                        var converted = ConvertToBsonCompatible(dataObj);
                        dataDict = converted as Dictionary<string, object>;
                    }

                    if (dataDict != null)
                    {
                        // Try common ID property names
                        var idPropertyNames = new[]
                        {
                            $"{entityType}Id",
                            $"{entityType.ToLowerInvariant()}Id",
                            "Id",
                            "id"
                        };

                        foreach (var propName in idPropertyNames)
                        {
                            if (dataDict.TryGetValue(propName, out var idObj))
                            {
                                // Convert to BSON-compatible and then to string
                                var converted = ConvertToBsonCompatible(idObj);
                                return converted?.ToString() ?? string.Empty;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting entity ID from event data: {Error}", ex.Message);
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts JsonElement and other non-BSON-compatible types to MongoDB-serializable types
    /// </summary>
    private object? ConvertToBsonCompatible(object? value)
    {
        if (value == null)
            return null;

        // If it's a JsonElement, convert it to a proper object
        if (value is JsonElement jsonElement)
        {
            return ConvertJsonElement(jsonElement);
        }

        // If it's a Dictionary with JsonElement values, convert them
        if (value is Dictionary<string, object> dict)
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                result[kvp.Key] = ConvertToBsonCompatible(kvp.Value) ?? kvp.Value;
            }
            return result;
        }

        // If it's a List/Array with JsonElement values, convert them
        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(ConvertToBsonCompatible(item));
            }
            return list;
        }

        // Primitive types and other BSON-compatible types can be returned as-is
        return value;
    }

    /// <summary>
    /// Converts a JsonElement to a BSON-compatible object (Dictionary, List, or primitive)
    /// </summary>
    private object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ConvertJsonElement(prop.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }
                return list;

            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case JsonValueKind.Number:
                // Try to get as int64 first, then double
                if (element.TryGetInt64(out var intValue))
                    return intValue;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return element.ToString();
        }
    }
}
