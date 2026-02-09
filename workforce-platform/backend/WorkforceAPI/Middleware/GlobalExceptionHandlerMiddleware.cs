using System.Net;
using System.Text.Json;
using Serilog;
using WorkforceAPI.Exceptions;

namespace WorkforceAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        HttpStatusCode statusCode;
        object response;

        switch (exception)
        {
            case EntityNotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                response = new { message = ex.Message };
                break;
            
            case ValidationException ex:
                statusCode = HttpStatusCode.BadRequest;
                response = new { message = ex.Message, propertyName = ex.PropertyName, attemptedValue = ex.AttemptedValue };
                break;
            
            case InvalidOperationException ex:
                statusCode = HttpStatusCode.BadRequest;
                response = new { message = ex.Message };
                break;
            
            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    message = "An unexpected error occurred",
                    error = _environment.IsDevelopment() ? exception.Message : null,
                    stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                    innerException = _environment.IsDevelopment() && exception.InnerException != null
                        ? exception.InnerException.Message
                        : null
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
