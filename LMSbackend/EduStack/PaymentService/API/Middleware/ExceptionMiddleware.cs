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

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (PaymentNotFoundException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (UnauthorizedPaymentAccessException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.Forbidden);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.Unauthorized);
        }
        catch (DomainException ex)
        {
            await HandleException(context, ex.Message, HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            await HandleException(context, "Internal Server Error", HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleException(HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { error = message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}