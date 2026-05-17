namespace DevConfTicketing.Domain.Vouchers;

public class Voucher
{
    public required string Id { get; init; }
    public required string EventId { get; init; }
    public required string Code { get; set; }
    public required DiscountType DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }
    public int MaxUsages { get; set; } = 1;
    public int UsedCount { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public List<string>? ApplicableTicketTypeIds { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; }
}
