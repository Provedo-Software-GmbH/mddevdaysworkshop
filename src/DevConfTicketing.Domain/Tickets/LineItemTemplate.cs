namespace DevConfTicketing.Domain.Tickets;

public class LineItemTemplate
{
    public required string Name { get; set; }
    public required decimal NetAmount { get; set; }
    public required string TaxRateId { get; set; }
    public required string TaxRateName { get; set; }
    public required decimal TaxRatePercentage { get; set; }
}
