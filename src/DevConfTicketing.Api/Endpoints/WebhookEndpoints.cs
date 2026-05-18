using DevConfTicketing.Infrastructure.Stripe;

namespace DevConfTicketing.Api.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/webhooks/stripe", async (HttpContext httpContext, StripeWebhookHandler webhookHandler, CancellationToken ct) =>
        {
            // Read raw body for signature verification
            using var reader = new StreamReader(httpContext.Request.Body);
            var payload = await reader.ReadToEndAsync(ct);

            if (!httpContext.Request.Headers.TryGetValue("Stripe-Signature", out var signatureHeader) ||
                string.IsNullOrEmpty(signatureHeader))
            {
                return Results.Problem(
                    detail: "Missing Stripe-Signature header.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await webhookHandler.ProcessAsync(payload, signatureHeader!, ct);

            return result switch
            {
                WebhookProcessingResult.SignatureInvalid => Results.Problem(
                    detail: "Invalid webhook signature.",
                    statusCode: StatusCodes.Status400BadRequest),
                WebhookProcessingResult.ProcessingError => Results.Problem(
                    detail: "Error processing webhook event.",
                    statusCode: StatusCodes.Status422UnprocessableEntity),
                _ => Results.Ok(new { received = true })
            };
        })
        .WithName("StripeWebhook")
        .WithTags("Webhooks")
        .WithDescription("Processes incoming Stripe webhook events with signature verification and idempotency")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
