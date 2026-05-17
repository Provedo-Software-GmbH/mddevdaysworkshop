using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Tickets;

[Description("Template for a line item used in ticket price composition")]
public class LineItemTemplate
{
    [Description("Display name of the line item")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [Description("Net amount before tax")]
    [JsonPropertyName("netAmount")]
    public required decimal NetAmount { get; set; }

    [Description("Identifier of the applied tax rate")]
    [JsonPropertyName("taxRateId")]
    public required string TaxRateId { get; set; }

    [Description("Display name of the applied tax rate")]
    [JsonPropertyName("taxRateName")]
    public required string TaxRateName { get; set; }

    [Description("Tax rate percentage applied to this line item")]
    [JsonPropertyName("taxRatePercentage")]
    public required decimal TaxRatePercentage { get; set; }
}
