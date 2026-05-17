using System.Diagnostics;

using DevConfTicketing.Application.Interfaces;

namespace DevConfTicketing.Api.Middleware;

public class RequestTelemetryMiddleware(RequestDelegate next, ITelemetryService telemetry)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var tags = new Dictionary<string, string>
            {
                ["http.method"] = context.Request.Method,
                ["http.status_code"] = context.Response.StatusCode.ToString(),
                ["http.route"] = context.GetEndpoint()?.DisplayName ?? context.Request.Path
            };

            telemetry.RecordHistogram("http.request.duration", stopwatch.Elapsed.TotalMilliseconds, tags);
            telemetry.IncrementCounter("http.requests", tags: tags);
        }
    }
}
