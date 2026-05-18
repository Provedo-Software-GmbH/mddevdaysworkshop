using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Api.Endpoints;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/{id}/checkout", async (string eventId, string id, CheckoutRequest request, IOrderRepository orderRepository, IPaymentService paymentService, CancellationToken ct) =>
        {
            var order = await orderRepository.GetByIdAsync(eventId, id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            if (order.Status is not OrderStatus.Pending)
            {
                return Results.Problem(
                    detail: "Only pending orders can proceed to checkout.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var result = await paymentService.CreateCheckoutSessionAsync(order, request.SuccessUrl, request.CancelUrl, ct);

            order.PaymentInfo = new PaymentInfo { StripeSessionId = result.SessionId };
            order.Status = OrderStatus.PaymentProcessing;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order, ct);

            return Results.Ok(new CheckoutResponse(result.SessionId, result.SessionUrl));
        })
        .WithName("CreateCheckoutSession")
        .WithTags("Payments")
        .WithDescription("Creates a Stripe Checkout Session for a pending order and returns the redirect URL")
        .Produces<CheckoutResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id}/payment-status", async (string eventId, string id, IOrderRepository orderRepository, IPaymentService paymentService, CancellationToken ct) =>
        {
            var order = await orderRepository.GetByIdAsync(eventId, id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            var status = await paymentService.GetPaymentStatusAsync(order, ct);
            return Results.Ok(new PaymentStatusResponse(order.Id, order.Status.ToString(), status.PaymentIntentId, status.PaidAt));
        })
        .WithName("GetPaymentStatus")
        .WithTags("Payments")
        .WithDescription("Gets the current payment status for an order")
        .Produces<PaymentStatusResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/cancel", async (string eventId, string id, IOrderRepository orderRepository, IPaymentService paymentService, CancellationToken ct) =>
        {
            var order = await orderRepository.GetByIdAsync(eventId, id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            if (order.Status is not OrderStatus.Paid)
            {
                return Results.Problem(
                    detail: "Only paid orders can be cancelled.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var hasCheckedInPositions = order.Positions.Any(p => p.CheckedInAt is not null);

            var refundResult = await paymentService.RefundAsync(order, ct);

            order.Status = OrderStatus.Cancelled;
            order.CancellationDate = DateTimeOffset.UtcNow;
            order.PaymentInfo!.StripeRefundId = refundResult.RefundId;
            order.PaymentInfo.RefundedAt = refundResult.RefundedAt;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order, ct);

            return Results.Ok(new CancelOrderResponse(order.Id, order.Status.ToString(), order.CancellationDate.Value, hasCheckedInPositions));
        })
        .WithName("CancelOrder")
        .WithTags("Payments")
        .WithDescription("Cancels a paid order and triggers a Stripe refund. Warns if tickets were already checked in.")
        .Produces<CancelOrderResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization("AdminPolicy");

        group.MapPost("/{id}/refund", async (string eventId, string id, IOrderRepository orderRepository, IPaymentService paymentService, CancellationToken ct) =>
        {
            var order = await orderRepository.GetByIdAsync(eventId, id, ct);
            if (order is null)
            {
                return Results.NotFound();
            }

            if (order.Status is not OrderStatus.Paid)
            {
                return Results.Problem(
                    detail: "Only paid orders can be refunded.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var refundResult = await paymentService.RefundAsync(order, ct);

            order.Status = OrderStatus.Refunded;
            order.PaymentInfo!.StripeRefundId = refundResult.RefundId;
            order.PaymentInfo.RefundedAt = refundResult.RefundedAt;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order, ct);

            return Results.Ok(new RefundOrderResponse(order.Id, order.Status.ToString(), refundResult.RefundId, refundResult.RefundedAt));
        })
        .WithName("RefundOrder")
        .WithTags("Payments")
        .WithDescription("Manually refunds a paid order via Stripe (admin action)")
        .Produces<RefundOrderResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization("AdminPolicy");

        return group;
    }
}

// --- Request DTOs ---

[Description("Request to initiate a Stripe Checkout session")]
public record CheckoutRequest(
    [property: Description("URL to redirect to after successful payment")]
    [property: JsonPropertyName("successUrl")]
    string SuccessUrl,

    [property: Description("URL to redirect to if payment is cancelled")]
    [property: JsonPropertyName("cancelUrl")]
    string CancelUrl
);

// --- Response DTOs ---

[Description("Response containing the Stripe Checkout session details")]
public record CheckoutResponse(
    [property: Description("Stripe Checkout Session ID")]
    [property: JsonPropertyName("sessionId")]
    string SessionId,

    [property: Description("URL to redirect the customer to for payment")]
    [property: JsonPropertyName("sessionUrl")]
    string SessionUrl
);

[Description("Response containing the current payment status of an order")]
public record PaymentStatusResponse(
    [property: Description("Order ID")]
    [property: JsonPropertyName("orderId")]
    string OrderId,

    [property: Description("Current order status")]
    [property: JsonPropertyName("status")]
    string Status,

    [property: Description("Stripe Payment Intent ID if available")]
    [property: JsonPropertyName("paymentIntentId")]
    string? PaymentIntentId,

    [property: Description("Timestamp when payment was completed")]
    [property: JsonPropertyName("paidAt")]
    DateTimeOffset? PaidAt
);

[Description("Response after cancelling an order")]
public record CancelOrderResponse(
    [property: Description("Order ID")]
    [property: JsonPropertyName("orderId")]
    string OrderId,

    [property: Description("New order status")]
    [property: JsonPropertyName("status")]
    string Status,

    [property: Description("Timestamp of cancellation")]
    [property: JsonPropertyName("cancellationDate")]
    DateTimeOffset CancellationDate,

    [property: Description("Whether any tickets were already checked in (warning)")]
    [property: JsonPropertyName("hasCheckedInPositions")]
    bool HasCheckedInPositions
);

[Description("Response after refunding an order")]
public record RefundOrderResponse(
    [property: Description("Order ID")]
    [property: JsonPropertyName("orderId")]
    string OrderId,

    [property: Description("New order status")]
    [property: JsonPropertyName("status")]
    string Status,

    [property: Description("Stripe Refund ID")]
    [property: JsonPropertyName("refundId")]
    string RefundId,

    [property: Description("Timestamp when refund was processed")]
    [property: JsonPropertyName("refundedAt")]
    DateTimeOffset RefundedAt
);
