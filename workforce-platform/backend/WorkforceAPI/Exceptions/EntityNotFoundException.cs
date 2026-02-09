namespace WorkforceAPI.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found in the database
/// </summary>
/// <remarks>
/// This exception is used throughout the application to indicate that a requested entity
/// (Employee, Project, Task, etc.) does not exist. It provides:
/// 1. Clear error messages for API consumers
/// 2. Entity type and ID for logging and debugging
/// 3. Automatic mapping to HTTP 404 (Not Found) status code via GlobalExceptionHandlerMiddleware
/// 
/// Usage example:
/// <code>
/// var employee = await _repository.GetByIdAsync(id);
/// if (employee == null)
/// {
///     throw new EntityNotFoundException("Employee", id);
/// }
/// </code>
/// 
/// The GlobalExceptionHandlerMiddleware automatically catches this exception and
/// returns a 404 Not Found response with a user-friendly message.
/// </remarks>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// The type of entity that was not found (e.g., "Employee", "Project", "Task")
    /// </summary>
    public string EntityType { get; }
    
    /// <summary>
    /// The ID that was used to search for the entity
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of EntityNotFoundException
    /// </summary>
    /// <param name="entityType">The type of entity that was not found</param>
    /// <param name="entityId">The ID that was used to search for the entity</param>
    /// <remarks>
    /// Creates an exception with a formatted message: "{EntityType} with ID {EntityId} not found"
    /// Example: "Employee with ID 123e4567-e89b-12d3-a456-426614174000 not found"
    /// </remarks>
    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID {entityId} not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of EntityNotFoundException with an inner exception
    /// </summary>
    /// <param name="entityType">The type of entity that was not found</param>
    /// <param name="entityId">The ID that was used to search for the entity</param>
    /// <param name="innerException">The exception that caused this exception</param>
    /// <remarks>
    /// Used when the entity lookup fails due to an underlying exception (e.g., database error).
    /// The inner exception is preserved for debugging purposes.
    /// </remarks>
    public EntityNotFoundException(string entityType, object entityId, Exception innerException)
        : base($"{entityType} with ID {entityId} not found", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
