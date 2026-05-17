using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Orders;

[Description("Represents a ticket order placed by a customer")]
public class Order
{
    [Description("Unique identifier of the order")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("Human-readable order code")]
    [JsonPropertyName("orderCode")]
    public required string OrderCode { get; init; }

    [Description("Identifier of the event the order belongs to")]
    [JsonPropertyName("eventId")]
    public required string EventId { get; init; }

    [Description("Email address of the customer")]
    [JsonPropertyName("customerEmail")]
    public required string CustomerEmail { get; set; }

    [Description("Full name of the customer")]
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [Description("Identifier of the registered customer")]
    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [Description("Voucher code applied to this order")]
    [JsonPropertyName("voucherCode")]
    public string? VoucherCode { get; set; }

    [Description("Current status of the order")]
    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Description("List of order positions containing individual tickets")]
    [JsonPropertyName("positions")]
    public List<OrderPosition> Positions { get; set; } = [];

    [Description("Total net amount before tax")]
    [JsonPropertyName("totalNet")]
    public required decimal TotalNet { get; set; }

    [Description("Total tax amount")]
    [JsonPropertyName("totalTax")]
    public required decimal TotalTax { get; set; }

    [Description("Total gross amount including tax")]
    [JsonPropertyName("totalGross")]
    public required decimal TotalGross { get; set; }

    [Description("Total discount amount applied")]
    [JsonPropertyName("discountAmount")]
    public required decimal DiscountAmount { get; set; }

    [Description("Currency code for all monetary amounts")]
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [Description("Timestamp when the order was created")]
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [Description("Timestamp when the order was last updated")]
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }

    [Description("Timestamp when the order was cancelled")]
    [JsonPropertyName("cancellationDate")]
    public DateTimeOffset? CancellationDate { get; set; }
}
