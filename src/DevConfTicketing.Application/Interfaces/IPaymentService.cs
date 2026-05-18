using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a Stripe Checkout Session for the given order.
    /// Returns the session URL for redirect.
    /// </summary>
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current payment status from Stripe for the given order.
    /// </summary>
    Task<PaymentStatusResult> GetPaymentStatusAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a full refund for a paid order via Stripe.
    /// </summary>
    Task<RefundResult> RefundAsync(Order order, CancellationToken cancellationToken = default);
}

public record CheckoutSessionResult(string SessionId, string SessionUrl);

public record PaymentStatusResult(string Status, string? PaymentIntentId, DateTimeOffset? PaidAt);

public record RefundResult(string RefundId, DateTimeOffset RefundedAt);
