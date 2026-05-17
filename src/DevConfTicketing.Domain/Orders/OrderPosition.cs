namespace DevConfTicketing.Domain.Orders;

public class OrderPosition
{
    public required int Index { get; init; }
    public required string TicketTypeId { get; init; }
    public required string TicketTypeName { get; init; }
    public required string TicketSecret { get; init; }
    public string? AttendeeName { get; set; }
    public string? AttendeeEmail { get; set; }
    public required List<OrderLineItem> LineItems { get; set; }
    public required decimal PositionNet { get; set; }
    public required decimal PositionTax { get; set; }
    public required decimal PositionGross { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }
    public string? CheckedInBy { get; set; }
}
