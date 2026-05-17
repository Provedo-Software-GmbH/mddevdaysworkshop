using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Tickets;

[Description("Represents a tax rate applicable to ticket line items")]
public class TaxRate
{
    [Description("Unique identifier of the tax rate")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("ISO country code the tax rate applies to")]
    [JsonPropertyName("countryCode")]
    public required string CountryCode { get; init; }

    [Description("Display name of the tax rate")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [Description("Tax rate percentage value")]
    [JsonPropertyName("percentage")]
    public required decimal Percentage { get; set; }

    [Description("Description of the tax rate")]
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [Description("Whether this is the default tax rate for the country")]
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [Description("Whether the tax rate is currently active")]
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}
