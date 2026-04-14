// Exception Middleware — Translates domain exceptions into standard HTTP responses.
// • Centralized error handling pipeline, catching errors before they crash the app.
// • 404 (Not Found): Quiz/Question missing.
// • 403 (Forbidden): Unauthorized access to someone else's attempt.
// • 400 (Bad Request): Business logic failures (e.g., submitting an already completed quiz).

using QuizService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace QuizService.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context); // continue processing the request
        }
        catch (QuizNotFoundException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (QuestionNotFoundException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (UnauthorizedQuizAccessException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.Forbidden);
        }
        catch (QuizSubmissionException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.BadRequest);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            // Generic 500 for unhandled exceptions to prevent leaking stack traces.
            await HandleException(context, "An unexpected error occurred: " + ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleException(HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        
        var response = new { success = false, error = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}