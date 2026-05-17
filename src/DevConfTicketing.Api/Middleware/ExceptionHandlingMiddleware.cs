using System.Diagnostics;
using System.Net;

using DevConfTicketing.Application.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace DevConfTicketing.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ITelemetryService telemetry, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        telemetry.TrackException(exception, new Dictionary<string, string>
        {
            ["endpoint"] = context.Request.Path,
            ["method"] = context.Request.Method
        });

        telemetry.IncrementCounter("http.errors", tags: new Dictionary<string, string>
        {
            ["endpoint"] = context.Request.Path,
            ["method"] = context.Request.Method,
            ["exception_type"] = exception.GetType().Name
        });

        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not Found"),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, "Conflict"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        // Include trace ID for correlation
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
