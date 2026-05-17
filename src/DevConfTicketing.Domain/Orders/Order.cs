namespace DevConfTicketing.Domain.Orders;

public class Order
{
    public required string Id { get; init; }
    public required string OrderCode { get; init; }
    public required string EventId { get; init; }
    public required string CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerId { get; set; }
    public string? VoucherCode { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required List<OrderPosition> Positions { get; set; }
    public required decimal TotalNet { get; set; }
    public required decimal TotalTax { get; set; }
    public required decimal TotalGross { get; set; }
    public required decimal DiscountAmount { get; set; }
    public required string Currency { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CancellationDate { get; set; }
}
