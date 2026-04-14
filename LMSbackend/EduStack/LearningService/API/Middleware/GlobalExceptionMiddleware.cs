// GlobalExceptionMiddleware — Centralized exception-to-HTTP mapping for Learning Service.
// • Registered FIRST in the pipeline to catch all errors from controllers and services.
// • Converts domain exceptions to structured JSON error responses (no crash pages in prod).
//
// Exception → HTTP mapping:
//   ProgressNotFoundException        → 404 Not Found (student hasn't watched lesson yet)
//   UnauthorizedLessonAccessException → 403 Forbidden (not enrolled in course)
//   UnauthorizedAccessException       → 401 Unauthorized (bad/missing JWT)
//   Anything else                     → 500 Internal Server Error

using LearningService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace LearningService.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // InvokeAsync — wraps the entire downstream pipeline in a try-catch.
    // If any controller, service, or repository throws, it's caught here and
    // converted to a clean JSON response instead of a 500 crash page.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // pass to next middleware / controller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    // HandleExceptionAsync — switch expression maps exception type → HTTP code.
    // Note: UnauthorizedLessonAccessException (403) is different from
    // UnauthorizedAccessException (401) — enrolled check vs unauthenticated.
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            ProgressNotFoundException        => (HttpStatusCode.NotFound, ex.Message),    // 404
            UnauthorizedLessonAccessException => (HttpStatusCode.Forbidden, ex.Message),  // 403
            UnauthorizedAccessException      => (HttpStatusCode.Unauthorized, ex.Message), // 401
            _                               => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        // Consistent JSON error shape for all error responses.
        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
