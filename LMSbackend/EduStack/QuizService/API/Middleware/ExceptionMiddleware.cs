using System.Net;
using System.Text.Json;
using QuizService.Domain.Exceptions;

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
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception ex)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = ex.Message;

        switch (ex)
        {
            case QuizNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;

            case QuestionNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;

            case UnauthorizedQuizAccessException:
                statusCode = HttpStatusCode.Forbidden;
                break;

            case QuizSubmissionException:
                statusCode = HttpStatusCode.BadRequest;
                break;
        }

        var response = new
        {
            status = (int)statusCode,
            error = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}