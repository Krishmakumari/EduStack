// GlobalExceptionMiddleware — Centralized error handling for the Course Service.
// • CourseNotFoundException → 404, UnauthorizedAccessException → 403, DomainException → 400.
// • Must be first in the middleware pipeline to catch all errors.

using CourseService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CourseService.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // InvokeAsync — called for every HTTP request.
    // Wraps the entire pipeline in a try-catch. If any downstream middleware
    // or controller throws, it's caught here and converted to a clean JSON response.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // pass request to next middleware / controller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    // HandleExceptionAsync — maps domain exceptions to HTTP status codes.
    // Uses C# switch expression (pattern matching) to determine the correct
    // status code based on the exception type. This is cleaner than if-else chains.
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        // Pattern matching: each domain exception maps to a specific HTTP code.
        // CourseNotFoundException → 404, UnauthorizedAccess → 403, DomainException → 400.
        // Anything else is an unexpected error → 500.
        var (statusCode, message) = ex switch
        {
            CourseNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),
            DomainException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        // Return a consistent JSON error shape for all error responses.
        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}