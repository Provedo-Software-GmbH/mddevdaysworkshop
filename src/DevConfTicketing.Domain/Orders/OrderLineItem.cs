namespace DevConfTicketing.Domain.Orders;

public class OrderLineItem
{
    public required string Name { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitNetAmount { get; init; }
    public required decimal TaxRatePercentage { get; init; }
    public required string TaxRateName { get; init; }
    public decimal TotalNet => UnitNetAmount * Quantity;
    public decimal TaxAmount => TotalNet * TaxRatePercentage / 100;
    public decimal TotalGross => TotalNet + TaxAmount;
}
