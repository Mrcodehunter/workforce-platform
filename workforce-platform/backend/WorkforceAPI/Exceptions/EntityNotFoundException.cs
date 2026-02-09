namespace WorkforceAPI.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : Exception
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID {entityId} not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, object entityId, Exception innerException)
        : base($"{entityType} with ID {entityId} not found", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
