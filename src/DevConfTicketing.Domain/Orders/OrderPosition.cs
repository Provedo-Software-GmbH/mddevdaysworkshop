using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Orders;

[Description("Represents an individual ticket position within an order")]
public class OrderPosition
{
    [Description("Zero-based index of the position within the order")]
    [JsonPropertyName("index")]
    public required int Index { get; init; }

    [Description("Identifier of the ticket type for this position")]
    [JsonPropertyName("ticketTypeId")]
    public required string TicketTypeId { get; init; }

    [Description("Display name of the ticket type")]
    [JsonPropertyName("ticketTypeName")]
    public required string TicketTypeName { get; init; }

    [Description("Secret code used for ticket validation and check-in")]
    [JsonPropertyName("ticketSecret")]
    public required string TicketSecret { get; init; }

    [Description("Name of the attendee assigned to this ticket")]
    [JsonPropertyName("attendeeName")]
    public string? AttendeeName { get; set; }

    [Description("Email of the attendee assigned to this ticket")]
    [JsonPropertyName("attendeeEmail")]
    public string? AttendeeEmail { get; set; }

    [Description("Line items composing the price of this position")]
    [JsonPropertyName("lineItems")]
    public required List<OrderLineItem> LineItems { get; set; }

    [Description("Net amount for this position before tax")]
    [JsonPropertyName("positionNet")]
    public required decimal PositionNet { get; set; }

    [Description("Tax amount for this position")]
    [JsonPropertyName("positionTax")]
    public required decimal PositionTax { get; set; }

    [Description("Gross amount for this position including tax")]
    [JsonPropertyName("positionGross")]
    public required decimal PositionGross { get; set; }

    [Description("Timestamp when the attendee checked in")]
    [JsonPropertyName("checkedInAt")]
    public DateTimeOffset? CheckedInAt { get; set; }

    [Description("Identifier of the person who performed the check-in")]
    [JsonPropertyName("checkedInBy")]
    public string? CheckedInBy { get; set; }
}
