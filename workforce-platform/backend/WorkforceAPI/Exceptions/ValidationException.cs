namespace WorkforceAPI.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public string? PropertyName { get; }
    public object? AttemptedValue { get; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, string? propertyName, object? attemptedValue = null)
        : base(message)
    {
        PropertyName = propertyName;
        AttemptedValue = attemptedValue;
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
