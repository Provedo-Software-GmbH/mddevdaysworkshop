using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Vouchers;

[Description("Represents a discount voucher applicable to an event order")]
public class Voucher
{
    [Description("Unique identifier of the voucher")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("Identifier of the event this voucher applies to")]
    [JsonPropertyName("eventId")]
    public required string EventId { get; init; }

    [Description("Voucher code entered by the customer")]
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [Description("Type of discount applied by this voucher")]
    [JsonPropertyName("discountType")]
    public required DiscountType DiscountType { get; set; }

    [Description("Numeric value of the discount")]
    [JsonPropertyName("discountValue")]
    public required decimal DiscountValue { get; set; }

    [Description("Maximum number of times this voucher can be used")]
    [JsonPropertyName("maxUsages")]
    public int MaxUsages { get; set; } = 1;

    [Description("Number of times this voucher has been used")]
    [JsonPropertyName("usedCount")]
    public int UsedCount { get; set; }

    [Description("Expiration date of the voucher")]
    [JsonPropertyName("validUntil")]
    public DateTimeOffset? ValidUntil { get; set; }

    [Description("List of ticket type identifiers this voucher can be applied to")]
    [JsonPropertyName("applicableTicketTypeIds")]
    public List<string>? ApplicableTicketTypeIds { get; set; }

    [Description("Whether the voucher is currently active")]
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [Description("Timestamp when the voucher was created")]
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }
}
