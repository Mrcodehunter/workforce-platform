using System.Net;
using System.Text.Json;
using Serilog;
using WorkforceAPI.Exceptions;

namespace WorkforceAPI.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
/// <remarks>
/// This middleware catches all unhandled exceptions in the request pipeline and converts them
/// into consistent JSON error responses. It provides:
/// 1. Centralized error handling (no need for try-catch in every controller)
/// 2. Consistent error response format across all endpoints
/// 3. Appropriate HTTP status codes based on exception type
/// 4. Security-conscious error messages (detailed errors only in development)
/// 
/// Exception to HTTP status code mapping:
/// - EntityNotFoundException -> 404 Not Found
/// - ValidationException -> 400 Bad Request
/// - InvalidOperationException -> 400 Bad Request
/// - All other exceptions -> 500 Internal Server Error
/// 
/// In development environments, detailed error information (stack traces, inner exceptions)
/// is included to aid debugging. In production, only generic error messages are returned
/// to prevent information leakage.
/// </remarks>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of GlobalExceptionHandlerMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="environment">The hosting environment (used to determine if detailed errors should be shown)</param>
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <remarks>
    /// This method wraps the request pipeline execution in a try-catch block.
    /// Any unhandled exception is caught, logged, and converted to a JSON error response.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to the next middleware in the pipeline
            // If an exception occurs, it will be caught below
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception for monitoring and debugging
            // This ensures all exceptions are logged even if they're handled gracefully
            Log.Error(ex, "Unhandled exception: {Message}", ex.Message);
            
            // Handle the exception and return appropriate HTTP response
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by converting them to appropriate HTTP responses
    /// </summary>
    /// <param name="context">The HTTP context for the current request</param>
    /// <param name="exception">The exception that was thrown</param>
    /// <remarks>
    /// This method:
    /// 1. Determines the appropriate HTTP status code based on exception type
    /// 2. Creates a response object with error details
    /// 3. Serializes the response to JSON
    /// 4. Writes the response to the HTTP context
    /// 
    /// The response format is consistent across all error types:
    /// - Success cases: Standard HTTP responses
    /// - Error cases: JSON object with "message" property (and optionally "error", "stackTrace", etc.)
    /// 
    /// Security consideration: Detailed error information (stack traces, inner exceptions)
    /// is only included in development environments to prevent information leakage in production.
    /// </remarks>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Set response content type to JSON
        context.Response.ContentType = "application/json";
        
        HttpStatusCode statusCode;
        object response;

        // Map exception types to HTTP status codes and create appropriate responses
        // This pattern matching ensures type-safe exception handling
        switch (exception)
        {
            case EntityNotFoundException ex:
                // Entity not found -> 404 Not Found
                // This indicates the requested resource doesn't exist
                statusCode = HttpStatusCode.NotFound;
                response = new { message = ex.Message };
                break;
            
            case ValidationException ex:
                // Validation failure -> 400 Bad Request
                // This indicates the client provided invalid data
                statusCode = HttpStatusCode.BadRequest;
                // Include property name and attempted value for detailed error reporting
                // This helps frontend applications highlight specific form fields
                response = new { message = ex.Message, propertyName = ex.PropertyName, attemptedValue = ex.AttemptedValue };
                break;
            
            case InvalidOperationException ex:
                // Invalid operation -> 400 Bad Request
                // This indicates the operation cannot be performed (e.g., business rule violation)
                statusCode = HttpStatusCode.BadRequest;
                response = new { message = ex.Message };
                break;
            
            default:
                // Unknown exception -> 500 Internal Server Error
                // This indicates an unexpected server error
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    message = "An unexpected error occurred",
                    // Only include detailed error information in development
                    // In production, this prevents information leakage to potential attackers
                    error = _environment.IsDevelopment() ? exception.Message : null,
                    stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                    innerException = _environment.IsDevelopment() && exception.InnerException != null
                        ? exception.InnerException.Message
                        : null
                };
                break;
        }

        // Set HTTP status code
        context.Response.StatusCode = (int)statusCode;

        // Configure JSON serialization options
        // CamelCase naming policy ensures consistency with JavaScript conventions
        // WriteIndented = true makes responses more readable (useful for debugging)
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Serialize response to JSON and write to HTTP response
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
