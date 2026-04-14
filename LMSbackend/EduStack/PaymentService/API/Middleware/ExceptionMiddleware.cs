// ExceptionMiddleware — Centralized exception-to-HTTP mapping for Payment Service.
// • Registered FIRST in the pipeline (before Auth and Controllers) to catch all errors.
// • Uses if-catch chain instead of switch expression — older style but equally correct.
// • Maps each domain exception type to its appropriate HTTP status code.
// • Generic Exception catch at the bottom — fallback for unexpected errors.
//
// Exception → HTTP mapping:
//   PaymentNotFoundException          → 404 Not Found
//   UnauthorizedPaymentAccessException → 403 Forbidden (authenticated but wrong student)
//   UnauthorizedAccessException        → 401 Unauthorized (missing/invalid JWT claim)
//   DomainException (base)            → 400 Bad Request (business rule violations)
//   Any other Exception               → 500 Internal Server Error

using PaymentService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace PaymentService.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Invoke — wraps the entire downstream pipeline in try-catch blocks.
    // Most specific exceptions are caught first (more derived → less derived order).
    // PaymentAlreadyCompletedException inherits from DomainException — caught by DomainException block.
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);   // pass to next middleware / controller
        }
        catch (PaymentNotFoundException ex)
        {
            // 404 — payment ID doesn't exist in the database.
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (UnauthorizedPaymentAccessException ex)
        {
            // 403 — student is authenticated but accessing another student's payment.
            await HandleException(context, ex.Message, HttpStatusCode.Forbidden);
        }
        catch (UnauthorizedAccessException ex)
        {
            // 401 — JWT claim missing (e.g., "sub" claim not found in token).
            await HandleException(context, ex.Message, HttpStatusCode.Unauthorized);
        }
        catch (DomainException ex)
        {
            // 400 — business rule violation: already purchased, invalid method,
            //        double-confirm, refund guard, etc.
            await HandleException(context, ex.Message, HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            // 500 — unexpected error (DB failure, null reference, etc.).
            // Message intentionally generic — don't expose internal details to clients.
            await HandleException(context, "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }

    // HandleException — writes a consistent JSON error response.
    // Shape: { "error": "message" } — simpler than other services (no "success" field).
    private static async Task HandleException(HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { error = message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}