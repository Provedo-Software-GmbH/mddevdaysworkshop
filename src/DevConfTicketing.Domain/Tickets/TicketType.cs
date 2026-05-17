using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Tickets;

[Description("Defines a category of ticket available for purchase")]
public class TicketType
{
    [Description("Unique identifier of the ticket type")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("Identifier of the event this ticket type belongs to")]
    [JsonPropertyName("eventId")]
    public required string EventId { get; init; }

    [Description("Display name of the ticket type")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [Description("Description of what this ticket type includes")]
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [Description("Price of the ticket")]
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [Description("Currency code for the ticket price")]
    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [Description("Number of tickets available for sale")]
    [JsonPropertyName("availableQuantity")]
    public int AvailableQuantity { get; set; }

    [Description("Number of tickets already sold")]
    [JsonPropertyName("soldQuantity")]
    public int SoldQuantity { get; set; }

    [Description("Maximum number of tickets allowed per order")]
    [JsonPropertyName("maxPerOrder")]
    public int MaxPerOrder { get; set; } = 10;

    [Description("Whether to display remaining ticket quantity to buyers")]
    [JsonPropertyName("showRemainingQuantity")]
    public bool ShowRemainingQuantity { get; set; }

    [Description("Date and time when ticket sales begin")]
    [JsonPropertyName("saleStart")]
    public DateTimeOffset? SaleStart { get; set; }

    [Description("Date and time when ticket sales end")]
    [JsonPropertyName("saleEnd")]
    public DateTimeOffset? SaleEnd { get; set; }

    [Description("Line item templates used to compose the ticket price breakdown")]
    [JsonPropertyName("lineItems")]
    public required List<LineItemTemplate> LineItems { get; set; }
}
