using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Orders;

[Description("Contains Stripe payment details associated with an order")]
public class PaymentInfo
{
    [Description("Stripe Checkout Session ID")]
    [JsonPropertyName("stripeSessionId")]
    public required string StripeSessionId { get; init; }

    [Description("Stripe Payment Intent ID, set after successful payment")]
    [JsonPropertyName("stripePaymentIntentId")]
    public string? StripePaymentIntentId { get; set; }

    [Description("Stripe Charge ID, set after successful charge")]
    [JsonPropertyName("stripeChargeId")]
    public string? StripeChargeId { get; set; }

    [Description("Stripe Refund ID, set after a refund is processed")]
    [JsonPropertyName("stripeRefundId")]
    public string? StripeRefundId { get; set; }

    [Description("Timestamp when payment was completed")]
    [JsonPropertyName("paidAt")]
    public DateTimeOffset? PaidAt { get; set; }

    [Description("Timestamp when refund was processed")]
    [JsonPropertyName("refundedAt")]
    public DateTimeOffset? RefundedAt { get; set; }
}
