using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Orders;

[Description("Represents a single line item within an order position")]
public class OrderLineItem
{
    [Description("Display name of the line item")]
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [Description("Quantity of units in this line item")]
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }

    [Description("Net amount per unit before tax")]
    [JsonPropertyName("unitNetAmount")]
    public required decimal UnitNetAmount { get; init; }

    [Description("Tax rate percentage applied to this line item")]
    [JsonPropertyName("taxRatePercentage")]
    public required decimal TaxRatePercentage { get; init; }

    [Description("Display name of the applied tax rate")]
    [JsonPropertyName("taxRateName")]
    public required string TaxRateName { get; init; }

    [Description("Computed total net amount")]
    [JsonPropertyName("totalNet")]
    public decimal TotalNet => UnitNetAmount * Quantity;

    [Description("Computed tax amount based on total net and tax rate")]
    [JsonPropertyName("taxAmount")]
    public decimal TaxAmount => TotalNet * TaxRatePercentage / 100;

    [Description("Computed total gross amount including tax")]
    [JsonPropertyName("totalGross")]
    public decimal TotalGross => TotalNet + TaxAmount;
}
