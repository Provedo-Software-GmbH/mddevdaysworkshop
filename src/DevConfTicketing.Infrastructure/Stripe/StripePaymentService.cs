using System.Diagnostics;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;
using Stripe.Checkout;

namespace DevConfTicketing.Infrastructure.Stripe;

public class StripePaymentService(
    IOptions<StripeOptions> options,
    ITelemetryService telemetry,
    ILogger<StripePaymentService> logger) : IPaymentService
{
    private readonly StripeOptions _options = options.Value;
    private readonly StripeClient _client = new(options.Value.SecretKey);

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan("Stripe.CreateCheckoutSession", ActivityKind.Client);
        span?.SetTag("order.id", order.Id);
        span?.SetTag("order.eventId", order.EventId);

        try
        {
            var lineItems = order.Positions.Select(position => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = _options.Currency,
                    UnitAmountDecimal = Math.Round(position.PositionGross * 100, 0, MidpointRounding.AwayFromZero),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = position.TicketTypeName,
                        Description = $"Order {order.OrderCode} - Position {position.Index + 1}"
                    }
                },
                Quantity = 1
            }).ToList();

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = order.Id,
                CustomerEmail = order.CustomerEmail,
                LineItems = lineItems,
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id,
                    ["eventId"] = order.EventId,
                    ["orderCode"] = order.OrderCode
                }
            };

            var service = new SessionService(_client);
            var session = await service.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

            telemetry.IncrementCounter("stripe.checkout_session.created");
            logger.LogInformation("Created Stripe Checkout Session {SessionId} for order {OrderId}", session.Id, order.Id);

            return new CheckoutSessionResult(session.Id, session.Url);
        }
        catch (StripeException ex)
        {
            telemetry.TrackException(ex);
            logger.LogError(ex, "Failed to create Stripe Checkout Session for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(Order order, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan("Stripe.GetPaymentStatus", ActivityKind.Client);
        span?.SetTag("order.id", order.Id);

        try
        {
            if (order.PaymentInfo is null)
            {
                return new PaymentStatusResult("no_payment", null, null);
            }

            var service = new SessionService(_client);
            var session = await service.GetAsync(order.PaymentInfo.StripeSessionId, cancellationToken: cancellationToken);

            var status = session.PaymentStatus switch
            {
                "paid" => "paid",
                "unpaid" => "unpaid",
                "no_payment_required" => "no_payment_required",
                _ => session.Status ?? "unknown"
            };

            string? paymentIntentId = session.PaymentIntentId;
            DateTimeOffset? paidAt = order.PaymentInfo.PaidAt;

            return new PaymentStatusResult(status, paymentIntentId, paidAt);
        }
        catch (StripeException ex)
        {
            telemetry.TrackException(ex);
            logger.LogError(ex, "Failed to get payment status for order {OrderId}", order.Id);
            throw;
        }
    }

    public async Task<RefundResult> RefundAsync(Order order, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan("Stripe.Refund", ActivityKind.Client);
        span?.SetTag("order.id", order.Id);

        try
        {
            if (order.PaymentInfo?.StripePaymentIntentId is null)
            {
                throw new InvalidOperationException($"Order {order.Id} has no Payment Intent ID — cannot refund.");
            }

            var service = new RefundService(_client);

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = order.PaymentInfo.StripePaymentIntentId,
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id,
                    ["eventId"] = order.EventId
                }
            };

            var refund = await service.CreateAsync(refundOptions, cancellationToken: cancellationToken);
            var refundedAt = DateTimeOffset.UtcNow;

            telemetry.IncrementCounter("stripe.refund.created");
            logger.LogInformation("Created Stripe Refund {RefundId} for order {OrderId}", refund.Id, order.Id);

            return new RefundResult(refund.Id, refundedAt);
        }
        catch (StripeException ex)
        {
            telemetry.TrackException(ex);
            logger.LogError(ex, "Failed to refund order {OrderId}", order.Id);
            throw;
        }
    }
}
