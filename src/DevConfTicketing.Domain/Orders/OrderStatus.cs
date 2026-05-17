namespace DevConfTicketing.Domain.Orders;

public enum OrderStatus
{
    Pending,
    PaymentProcessing,
    Paid,
    Cancelled,
    Refunded
}
