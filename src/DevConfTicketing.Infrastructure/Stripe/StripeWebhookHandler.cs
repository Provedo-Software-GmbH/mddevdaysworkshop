using System.Diagnostics;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Stripe;

namespace DevConfTicketing.Infrastructure.Stripe;

public class StripeWebhookHandler(
    IOptions<StripeOptions> options,
    IOrderRepository orderRepository,
    IWebhookEventRepository webhookEventRepository,
    ITelemetryService telemetry,
    ILogger<StripeWebhookHandler> logger)
{
    private readonly StripeOptions _options = options.Value;

    /// <summary>
    /// Processes a raw Stripe webhook payload with signature verification and idempotency.
    /// </summary>
    public async Task<WebhookProcessingResult> ProcessAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan("Stripe.Webhook.Process", ActivityKind.Consumer);

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _options.WebhookSecret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed");
            telemetry.IncrementCounter("stripe.webhook.signature_failed");
            return WebhookProcessingResult.SignatureInvalid;
        }

        span?.SetTag("stripe.event.id", stripeEvent.Id);
        span?.SetTag("stripe.event.type", stripeEvent.Type);

        logger.LogInformation("Processing Stripe webhook event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);

        return stripeEvent.Type switch
        {
            "checkout.session.completed" => await HandleCheckoutSessionCompletedAsync(stripeEvent, cancellationToken),
            "checkout.session.expired" => await HandleCheckoutSessionExpiredAsync(stripeEvent, cancellationToken),
            "charge.refunded" => await HandleChargeRefundedAsync(stripeEvent, cancellationToken),
            _ => HandleUnknownEvent(stripeEvent)
        };
    }

    private async Task<WebhookProcessingResult> HandleCheckoutSessionCompletedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        using var span = telemetry.StartSpan("Stripe.Webhook.CheckoutSessionCompleted", ActivityKind.Internal);

        var session = stripeEvent.Data.Object as global::Stripe.Checkout.Session;
        if (session is null)
        {
            logger.LogError("Failed to deserialize checkout.session.completed event {EventId}", stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        var orderId = session.Metadata.GetValueOrDefault("orderId");
        var eventId = session.Metadata.GetValueOrDefault("eventId");

        if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(eventId))
        {
            logger.LogError("Missing metadata (orderId/eventId) in checkout.session.completed event {EventId}", stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        // Idempotency check
        if (await webhookEventRepository.ExistsAsync(eventId, stripeEvent.Id, cancellationToken))
        {
            logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
            telemetry.IncrementCounter("stripe.webhook.duplicate");
            return WebhookProcessingResult.AlreadyProcessed;
        }

        var order = await orderRepository.GetByIdAsync(eventId, orderId, cancellationToken);
        if (order is null)
        {
            logger.LogError("Order {OrderId} not found for checkout.session.completed event {EventId}", orderId, stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        // Only transition from PaymentProcessing → Paid
        if (order.Status is not OrderStatus.PaymentProcessing)
        {
            logger.LogWarning("Order {OrderId} is in status {Status}, expected PaymentProcessing. Skipping.", orderId, order.Status);
            return WebhookProcessingResult.AlreadyProcessed;
        }

        order.Status = OrderStatus.Paid;
        order.PaymentInfo ??= new PaymentInfo { StripeSessionId = session.Id };
        order.PaymentInfo.StripePaymentIntentId = session.PaymentIntentId;
        order.PaymentInfo.PaidAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await orderRepository.UpdateAsync(order, cancellationToken);

        // Record idempotency
        await webhookEventRepository.CreateAsync(new StripeWebhookEvent
        {
            Id = stripeEvent.Id,
            EventType = stripeEvent.Type,
            OrderId = orderId,
            EventId = eventId,
            ProcessedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        telemetry.IncrementCounter("stripe.webhook.checkout_completed");
        logger.LogInformation("Order {OrderId} marked as Paid via webhook event {WebhookEventId}", orderId, stripeEvent.Id);

        return WebhookProcessingResult.Success;
    }

    private async Task<WebhookProcessingResult> HandleCheckoutSessionExpiredAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        using var span = telemetry.StartSpan("Stripe.Webhook.CheckoutSessionExpired", ActivityKind.Internal);

        var session = stripeEvent.Data.Object as global::Stripe.Checkout.Session;
        if (session is null)
        {
            logger.LogError("Failed to deserialize checkout.session.expired event {EventId}", stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        var orderId = session.Metadata.GetValueOrDefault("orderId");
        var eventId = session.Metadata.GetValueOrDefault("eventId");

        if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(eventId))
        {
            logger.LogError("Missing metadata (orderId/eventId) in checkout.session.expired event {EventId}", stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        // Idempotency check
        if (await webhookEventRepository.ExistsAsync(eventId, stripeEvent.Id, cancellationToken))
        {
            logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
            telemetry.IncrementCounter("stripe.webhook.duplicate");
            return WebhookProcessingResult.AlreadyProcessed;
        }

        var order = await orderRepository.GetByIdAsync(eventId, orderId, cancellationToken);
        if (order is null)
        {
            logger.LogError("Order {OrderId} not found for checkout.session.expired event {EventId}", orderId, stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        if (order.Status is not OrderStatus.PaymentProcessing)
        {
            logger.LogWarning("Order {OrderId} is in status {Status}, expected PaymentProcessing. Skipping.", orderId, order.Status);
            return WebhookProcessingResult.AlreadyProcessed;
        }

        order.Status = OrderStatus.Cancelled;
        order.CancellationDate = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await orderRepository.UpdateAsync(order, cancellationToken);

        // Record idempotency
        await webhookEventRepository.CreateAsync(new StripeWebhookEvent
        {
            Id = stripeEvent.Id,
            EventType = stripeEvent.Type,
            OrderId = orderId,
            EventId = eventId,
            ProcessedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        telemetry.IncrementCounter("stripe.webhook.checkout_expired");
        logger.LogInformation("Order {OrderId} marked as Cancelled (session expired) via webhook event {WebhookEventId}", orderId, stripeEvent.Id);

        return WebhookProcessingResult.Success;
    }

    private async Task<WebhookProcessingResult> HandleChargeRefundedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        using var span = telemetry.StartSpan("Stripe.Webhook.ChargeRefunded", ActivityKind.Internal);

        var charge = stripeEvent.Data.Object as Charge;
        if (charge is null)
        {
            logger.LogError("Failed to deserialize charge.refunded event {EventId}", stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        var orderId = charge.Metadata.GetValueOrDefault("orderId");
        var eventId = charge.Metadata.GetValueOrDefault("eventId");

        if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(eventId))
        {
            // Try to find the order via PaymentIntent metadata if charge metadata is missing
            logger.LogWarning("Missing metadata in charge.refunded event {EventId}, event acknowledged without processing", stripeEvent.Id);
            return WebhookProcessingResult.Success;
        }

        // Idempotency check
        if (await webhookEventRepository.ExistsAsync(eventId, stripeEvent.Id, cancellationToken))
        {
            logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
            telemetry.IncrementCounter("stripe.webhook.duplicate");
            return WebhookProcessingResult.AlreadyProcessed;
        }

        var order = await orderRepository.GetByIdAsync(eventId, orderId, cancellationToken);
        if (order is null)
        {
            logger.LogError("Order {OrderId} not found for charge.refunded event {EventId}", orderId, stripeEvent.Id);
            return WebhookProcessingResult.ProcessingError;
        }

        // Only transition Paid → Refunded (cancel/refund endpoints may have already handled this)
        if (order.Status is not OrderStatus.Paid)
        {
            logger.LogInformation("Order {OrderId} is already in status {Status}, acknowledging refund webhook", orderId, order.Status);
            return WebhookProcessingResult.AlreadyProcessed;
        }

        order.Status = OrderStatus.Refunded;
        if (order.PaymentInfo is not null)
        {
            order.PaymentInfo.RefundedAt = DateTimeOffset.UtcNow;
        }
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await orderRepository.UpdateAsync(order, cancellationToken);

        // Record idempotency
        await webhookEventRepository.CreateAsync(new StripeWebhookEvent
        {
            Id = stripeEvent.Id,
            EventType = stripeEvent.Type,
            OrderId = orderId,
            EventId = eventId,
            ProcessedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        telemetry.IncrementCounter("stripe.webhook.charge_refunded");
        logger.LogInformation("Order {OrderId} marked as Refunded via webhook event {WebhookEventId}", orderId, stripeEvent.Id);

        return WebhookProcessingResult.Success;
    }

    private WebhookProcessingResult HandleUnknownEvent(Event stripeEvent)
    {
        logger.LogDebug("Unhandled Stripe event type {EventType} with ID {EventId}", stripeEvent.Type, stripeEvent.Id);
        return WebhookProcessingResult.Success;
    }
}

public enum WebhookProcessingResult
{
    Success,
    AlreadyProcessed,
    SignatureInvalid,
    ProcessingError
}
