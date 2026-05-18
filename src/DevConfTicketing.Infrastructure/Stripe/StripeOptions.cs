using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Infrastructure.Stripe;

[Description("Configuration options for Stripe payment integration")]
public class StripeOptions
{
    public const string SectionName = "Stripe";

    [Description("Stripe Secret API Key")]
    [JsonPropertyName("secretKey")]
    public required string SecretKey { get; init; }

    [Description("Stripe Webhook signing secret for signature verification")]
    [JsonPropertyName("webhookSecret")]
    public required string WebhookSecret { get; init; }

    [Description("Currency code for Stripe charges (e.g. eur)")]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "eur";
}
