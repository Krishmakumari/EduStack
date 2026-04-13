// GlobalExceptionMiddleware — Centralized error handling for all requests.
// • Wraps every HTTP request in try-catch; controllers throw freely.
// • Maps exceptions to HTTP status codes: 401, 403, 400, or 500.
// • Must be FIRST in the middleware pipeline to catch all errors.

using AuthService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AuthService.API.Middleware;

public class GlobalExceptionMiddleware
{
    // _next is the next middleware in the pipeline. We call it inside try-catch.
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Called for EVERY HTTP request. Wraps the entire pipeline in try-catch.
    /// If no exception is thrown, the request flows normally.
    /// If an exception IS thrown, we catch it and return a clean JSON error.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass the request to the next middleware (eventually reaching the controller).
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging (stack trace, inner exception, etc.)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            // Convert the exception to a proper HTTP error response.
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps domain exceptions to HTTP status codes using C# pattern matching.
    /// This is a clean, maintainable approach — add new exception types here as needed.
    /// </summary>
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        // Pattern matching (C# switch expression) — maps exception TYPE to status code.
        // More specific types are checked FIRST (they inherit from DomainException).
        var (statusCode, message) = ex switch
        {
            InvalidCredentialsException => (HttpStatusCode.Unauthorized, ex.Message),
            AccountBannedException => (HttpStatusCode.Forbidden, ex.Message),
            EmailNotVerifiedException => (HttpStatusCode.Forbidden, ex.Message),
            DomainException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            // ↑ For unknown errors, we return a generic message (don't leak internal details!)
        };

        context.Response.StatusCode = (int)statusCode;

        // Consistent error response shape — the frontend always knows what to expect.
        var response = new
        {
            success = false,
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}