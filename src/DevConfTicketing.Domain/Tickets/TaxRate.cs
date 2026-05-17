namespace DevConfTicketing.Domain.Tickets;

public class TaxRate
{
    public required string Id { get; init; }
    public required string CountryCode { get; init; }
    public required string Name { get; set; }
    public required decimal Percentage { get; set; }
    public required string Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
