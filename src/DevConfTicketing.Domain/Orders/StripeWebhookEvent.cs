using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Orders;

[Description("Stores processed Stripe webhook event IDs for idempotency")]
public class StripeWebhookEvent
{
    [Description("Unique identifier matching the Stripe event ID")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("Type of Stripe event (e.g. checkout.session.completed)")]
    [JsonPropertyName("eventType")]
    public required string EventType { get; init; }

    [Description("Associated order ID")]
    [JsonPropertyName("orderId")]
    public required string OrderId { get; init; }

    [Description("Associated event ID used as partition key")]
    [JsonPropertyName("eventId")]
    public required string EventId { get; init; }

    [Description("Timestamp when the webhook event was processed")]
    [JsonPropertyName("processedAt")]
    public DateTimeOffset ProcessedAt { get; init; }
}
