using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Tests.TestHelpers;

/// <summary>
/// Fluent builder for creating test <see cref="Order"/> instances with sensible defaults.
/// </summary>
public class OrderBuilder
{
    private string _id = $"order-{Guid.NewGuid():N}";
    private string _orderCode = "MDDD-TEST-0001";
    private string _eventId = "event-1";
    private string _customerEmail = "test@example.com";
    private string? _customerName = "Test Customer";
    private OrderStatus _status = OrderStatus.Pending;
    private decimal _totalNet = 100.00m;
    private decimal _totalTax = 19.00m;
    private decimal _totalGross = 119.00m;
    private decimal _discountAmount = 0m;
    private string _currency = "EUR";
    private PaymentInfo? _paymentInfo;
    private DateTimeOffset? _cancellationDate;
    private List<OrderPosition> _positions = [];

    public OrderBuilder WithId(string id) { _id = id; return this; }
    public OrderBuilder WithOrderCode(string code) { _orderCode = code; return this; }
    public OrderBuilder WithEventId(string eventId) { _eventId = eventId; return this; }
    public OrderBuilder WithCustomerEmail(string email) { _customerEmail = email; return this; }
    public OrderBuilder WithStatus(OrderStatus status) { _status = status; return this; }
    public OrderBuilder WithTotals(decimal net, decimal tax, decimal gross) { _totalNet = net; _totalTax = tax; _totalGross = gross; return this; }
    public OrderBuilder WithDiscount(decimal amount) { _discountAmount = amount; return this; }
    public OrderBuilder WithPaymentInfo(PaymentInfo paymentInfo) { _paymentInfo = paymentInfo; return this; }
    public OrderBuilder WithCancellationDate(DateTimeOffset date) { _cancellationDate = date; return this; }

    public OrderBuilder WithPosition(string ticketTypeId = "tt-1", string ticketTypeName = "Full Pass", decimal positionGross = 119.00m)
    {
        _positions.Add(new OrderPosition
        {
            Index = _positions.Count,
            TicketTypeId = ticketTypeId,
            TicketTypeName = ticketTypeName,
            TicketSecret = $"secret-{Guid.NewGuid():N}",
            LineItems =
            [
                new OrderLineItem
                {
                    Name = ticketTypeName,
                    Quantity = 1,
                    UnitNetAmount = Math.Round(positionGross / 1.19m, 2),
                    TaxRatePercentage = 19m,
                    TaxRateName = "MwSt. 19%"
                }
            ],
            PositionNet = Math.Round(positionGross / 1.19m, 2),
            PositionTax = positionGross - Math.Round(positionGross / 1.19m, 2),
            PositionGross = positionGross
        });
        return this;
    }

    /// <summary>
    /// Creates a typical order in PaymentProcessing status with a Stripe session.
    /// </summary>
    public OrderBuilder InPaymentProcessing(string sessionId = "cs_test_session_123")
    {
        _status = OrderStatus.PaymentProcessing;
        _paymentInfo = new PaymentInfo { StripeSessionId = sessionId };
        return WithPosition();
    }

    /// <summary>
    /// Creates a typical paid order.
    /// </summary>
    public OrderBuilder AsPaid(string sessionId = "cs_test_session_123", string paymentIntentId = "pi_test_123")
    {
        _status = OrderStatus.Paid;
        _paymentInfo = new PaymentInfo
        {
            StripeSessionId = sessionId,
            StripePaymentIntentId = paymentIntentId,
            PaidAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        return WithPosition();
    }

    public Order Build() => new()
    {
        Id = _id,
        OrderCode = _orderCode,
        EventId = _eventId,
        CustomerEmail = _customerEmail,
        CustomerName = _customerName,
        Status = _status,
        Positions = _positions.Count > 0 ? _positions : [new OrderPosition
        {
            Index = 0,
            TicketTypeId = "tt-1",
            TicketTypeName = "Full Pass",
            TicketSecret = "secret-default",
            LineItems = [new OrderLineItem { Name = "Conference", Quantity = 1, UnitNetAmount = _totalNet, TaxRatePercentage = 19m, TaxRateName = "MwSt. 19%" }],
            PositionNet = _totalNet,
            PositionTax = _totalTax,
            PositionGross = _totalGross
        }],
        TotalNet = _totalNet,
        TotalTax = _totalTax,
        TotalGross = _totalGross,
        DiscountAmount = _discountAmount,
        Currency = _currency,
        CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
        UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1),
        CancellationDate = _cancellationDate,
        PaymentInfo = _paymentInfo
    };
}
