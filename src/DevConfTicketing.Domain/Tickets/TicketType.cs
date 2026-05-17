namespace DevConfTicketing.Domain.Tickets;

public class TicketType
{
    public required string Id { get; init; }
    public required string EventId { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required string Currency { get; set; }
    public int AvailableQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int MaxPerOrder { get; set; } = 10;
    public bool ShowRemainingQuantity { get; set; }
    public DateTimeOffset? SaleStart { get; set; }
    public DateTimeOffset? SaleEnd { get; set; }
    public required List<LineItemTemplate> LineItems { get; set; }
}
