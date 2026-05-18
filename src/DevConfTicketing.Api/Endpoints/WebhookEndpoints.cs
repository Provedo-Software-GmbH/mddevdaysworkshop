using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Api.Endpoints;

public static class WebhookEndpoints
{
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/webhooks/stripe", async (HttpContext httpContext, IOrderRepository orderRepository, IWebhookEventRepository webhookEventRepository, CancellationToken ct) =>
        {
            // Stub: Read the raw body, verify Stripe signature, dispatch to handlers
            // Implementation will:
            // 1. Verify webhook signature using STRIPE_WEBHOOK_SECRET
            // 2. Check idempotency (webhookEventRepository.ExistsAsync)
            // 3. Handle event types:
            //    - checkout.session.completed → Order.Status = Paid
            //    - checkout.session.expired → Order.Status = Cancelled
            //    - charge.refunded → Order.Status = Refunded
            // 4. Store processed event for idempotency

            return Results.Ok(new { received = true });
        })
        .WithName("StripeWebhook")
        .WithTags("Webhooks")
        .WithDescription("Processes incoming Stripe webhook events with signature verification and idempotency")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
