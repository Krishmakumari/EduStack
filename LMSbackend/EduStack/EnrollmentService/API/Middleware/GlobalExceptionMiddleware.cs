// GlobalExceptionMiddleware — Centralized error handling for the Enrollment Service.
// • Maps domain exceptions to HTTP status codes for consistent JSON error responses.
// • Must be registered FIRST in the pipeline to catch errors from all middleware below.
//
// Exception → HTTP mapping:
//   EnrollmentNotFoundException        → 404 Not Found
//   AlreadyEnrolledException           → 409 Conflict
//   UnauthorizedEnrollmentAccessException → 403 Forbidden
//   UnauthorizedAccessException        → 401 Unauthorized
//   DomainException (base)            → 400 Bad Request
//   Anything else                      → 500 Internal Server Error

using EnrollmentService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace EnrollmentService.API.Middleware;

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
    // Wraps downstream pipeline in try-catch. Any unhandled exception is caught here
    // and converted to a structured JSON error response instead of a crash page.
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

    // HandleExceptionAsync — maps exception types to HTTP status codes.
    // Switch expression pattern matching: cleaner than if-else chains.
    // Note: AlreadyEnrolledException → 409 Conflict (not 400) because the request
    // is syntactically valid — it's a business conflict (already enrolled).
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            EnrollmentNotFoundException          => (HttpStatusCode.NotFound, ex.Message),
            AlreadyEnrolledException             => (HttpStatusCode.Conflict, ex.Message),      // 409
            UnauthorizedEnrollmentAccessException => (HttpStatusCode.Forbidden, ex.Message),    // 403
            UnauthorizedAccessException          => (HttpStatusCode.Unauthorized, ex.Message),  // 401
            DomainException                      => (HttpStatusCode.BadRequest, ex.Message),    // 400
            _                                    => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        // Consistent JSON shape for all error responses.
        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}