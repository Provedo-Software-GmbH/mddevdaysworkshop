using System.Diagnostics;
using System.Text.Json;

using DevConfTicketing.Application.Interfaces;

namespace DevConfTicketing.Api.Endpoints;

public static class TelemetryEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/telemetry", async (HttpRequest request, ITelemetryService telemetry) =>
        {
            using var span = telemetry.StartSpan("TelemetryProxy", ActivityKind.Server);

            try
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return Results.BadRequest(new { error = "Empty telemetry payload" });
                }

                // Validate it's valid JSON (OTLP/HTTP JSON format)
                using var document = JsonDocument.Parse(body);

                // Log the received telemetry for the OpenTelemetry pipeline to pick up
                span?.SetTag("telemetry.source", "frontend");
                span?.SetTag("telemetry.payload_size", body.Length);

                telemetry.IncrementCounter("telemetry.proxy.received", tags: new Dictionary<string, string>
                {
                    ["source"] = "frontend"
                });

                return Results.Accepted();
            }
            catch (JsonException)
            {
                return Results.BadRequest(new { error = "Invalid JSON payload" });
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ProxyFrontendTelemetry")
        .WithTags("Telemetry")
        .WithDescription("Accepts OTLP/HTTP JSON traces from the browser and re-exports via the backend's OpenTelemetry pipeline")
        .Accepts<object>("application/json")
        .Produces(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
