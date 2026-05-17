using System.ComponentModel;

namespace DevConfTicketing.Domain.Orders;

[Description("Represents the lifecycle status of an order")]
public enum OrderStatus
{
    [Description("Order has been placed and is awaiting payment")]
    Pending,

    [Description("Payment is currently being processed")]
    PaymentProcessing,

    [Description("Order has been paid successfully")]
    Paid,

    [Description("Order has been cancelled")]
    Cancelled,

    [Description("Order has been refunded")]
    Refunded
}
