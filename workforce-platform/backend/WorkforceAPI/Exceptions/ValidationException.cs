namespace WorkforceAPI.Exceptions;

/// <summary>
/// Exception thrown when validation fails for input data
/// </summary>
/// <remarks>
/// This exception is used to indicate that input validation has failed. It provides:
/// 1. Clear error messages for API consumers
/// 2. Property name and attempted value for detailed error reporting
/// 3. Automatic mapping to HTTP 400 (Bad Request) status code via GlobalExceptionHandlerMiddleware
/// 
/// This exception is typically thrown by:
/// - Service layer when business rule validation fails
/// - Custom validation logic that can't be expressed in FluentValidation
/// - Manual validation checks before database operations
/// 
/// Usage example:
/// <code>
/// if (id == Guid.Empty)
/// {
///     throw new ValidationException("Employee ID is required", nameof(id));
/// }
/// </code>
/// 
/// The GlobalExceptionHandlerMiddleware automatically catches this exception and
/// returns a 400 Bad Request response with validation details.
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// The name of the property that failed validation (if applicable)
    /// </summary>
    /// <remarks>
    /// This property is used to provide field-specific error messages in API responses.
    /// It helps frontend applications highlight the specific field that has an error.
    /// </remarks>
    public string? PropertyName { get; }
    
    /// <summary>
    /// The value that was attempted (if applicable)
    /// </summary>
    /// <remarks>
    /// This property stores the invalid value that was provided.
    /// It's useful for debugging and can be included in error responses.
    /// </remarks>
    public object? AttemptedValue { get; }

    /// <summary>
    /// Initializes a new instance of ValidationException with a message
    /// </summary>
    /// <param name="message">The validation error message</param>
    /// <remarks>
    /// Use this constructor when you only need to provide a general validation error message
    /// without specific property or value details.
    /// </remarks>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with message, property name, and attempted value
    /// </summary>
    /// <param name="message">The validation error message</param>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="attemptedValue">The value that was attempted (optional)</param>
    /// <remarks>
    /// Use this constructor when you want to provide detailed validation information
    /// including which property failed and what value was provided.
    /// 
    /// Example:
    /// <code>
    /// throw new ValidationException("Email is required", nameof(email), email);
    /// </code>
    /// </remarks>
    public ValidationException(string message, string? propertyName, object? attemptedValue = null)
        : base(message)
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with message and inner exception
    /// </summary>
    /// <param name="message">The validation error message</param>
    /// <param name="innerException">The exception that caused this validation exception</param>
    /// <remarks>
    /// Use this constructor when validation fails due to an underlying exception
    /// (e.g., database constraint violation, external service error).
    /// The inner exception is preserved for debugging purposes.
    /// </remarks>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
