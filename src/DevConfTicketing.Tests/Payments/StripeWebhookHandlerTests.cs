using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;
using DevConfTicketing.Infrastructure.Stripe;
using DevConfTicketing.Tests.TestHelpers;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Stripe;

namespace DevConfTicketing.Tests.Payments;

public class StripeWebhookHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger<StripeWebhookHandler> _logger;
    private readonly StripeWebhookHandler _handler;

    private const string TestWebhookSecret = "whsec_test_secret";
    private const string TestEventId = "event-1";
    private const string TestOrderId = "order-1";
    private const string TestStripeEventId = "evt_test_123";
    private const string TestSessionId = "cs_test_session_456";
    private const string TestPaymentIntentId = "pi_test_789";

    public StripeWebhookHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _webhookEventRepository = Substitute.For<IWebhookEventRepository>();
        _telemetry = new NullTelemetryService();
        _logger = Substitute.For<ILogger<StripeWebhookHandler>>();

        var options = Options.Create(new StripeOptions
        {
            SecretKey = "sk_test_xxx",
            WebhookSecret = TestWebhookSecret,
            Currency = "eur"
        });

        _handler = new StripeWebhookHandler(
            options,
            _orderRepository,
            _webhookEventRepository,
            _telemetry,
            _logger);
    }

    #region Idempotency Tests

    [Fact]
    public async Task ProcessAsync_DuplicateCheckoutCompleted_ReturnsAlreadyProcessed()
    {
        // Arrange - event was already processed
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(true);

        var (payload, signature) = CreateCheckoutSessionCompletedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.AlreadyProcessed);
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_DuplicateCheckoutExpired_ReturnsAlreadyProcessed()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(true);

        var (payload, signature) = CreateCheckoutSessionExpiredEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.AlreadyProcessed);
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_DuplicateChargeRefunded_ReturnsAlreadyProcessed()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(true);

        var (payload, signature) = CreateChargeRefundedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.AlreadyProcessed);
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_AfterSuccessfulProcessing_RecordsWebhookEvent()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .InPaymentProcessing(TestSessionId)
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateCheckoutSessionCompletedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.Success);
        await _webhookEventRepository.Received(1).CreateAsync(
            Arg.Is<StripeWebhookEvent>(e =>
                e.Id == TestStripeEventId &&
                e.EventType == "checkout.session.completed" &&
                e.OrderId == TestOrderId &&
                e.EventId == TestEventId),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Checkout Session Completed Tests

    [Fact]
    public async Task ProcessAsync_CheckoutCompleted_TransitionsOrderToPaid()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .InPaymentProcessing(TestSessionId)
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateCheckoutSessionCompletedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.Success);
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaymentInfo.Should().NotBeNull();
        order.PaymentInfo!.StripePaymentIntentId.Should().Be(TestPaymentIntentId);
        order.PaymentInfo.PaidAt.Should().NotBeNull();
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_CheckoutCompleted_OrderNotInPaymentProcessing_ReturnsAlreadyProcessed()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .AsPaid()
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateCheckoutSessionCompletedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.AlreadyProcessed);
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_CheckoutCompleted_OrderNotFound_ReturnsProcessingError()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var (payload, signature) = CreateCheckoutSessionCompletedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.ProcessingError);
    }

    #endregion

    #region Checkout Session Expired Tests

    [Fact]
    public async Task ProcessAsync_CheckoutExpired_TransitionsOrderToCancelled()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .InPaymentProcessing(TestSessionId)
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateCheckoutSessionExpiredEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.Success);
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationDate.Should().NotBeNull();
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Charge Refunded Tests

    [Fact]
    public async Task ProcessAsync_ChargeRefunded_TransitionsOrderToRefunded()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .AsPaid(TestSessionId, TestPaymentIntentId)
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateChargeRefundedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.Success);
        order.Status.Should().Be(OrderStatus.Refunded);
        order.PaymentInfo!.RefundedAt.Should().NotBeNull();
        await _orderRepository.Received(1).UpdateAsync(order, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ChargeRefunded_OrderNotPaid_ReturnsAlreadyProcessed()
    {
        // Arrange
        _webhookEventRepository.ExistsAsync(TestEventId, TestStripeEventId, Arg.Any<CancellationToken>())
            .Returns(false);

        var order = new OrderBuilder()
            .WithId(TestOrderId)
            .WithEventId(TestEventId)
            .WithStatus(OrderStatus.Refunded)
            .Build();

        _orderRepository.GetByIdAsync(TestEventId, TestOrderId, Arg.Any<CancellationToken>())
            .Returns(order);

        var (payload, signature) = CreateChargeRefundedEvent();

        // Act
        var result = await _handler.ProcessAsync(payload, signature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.AlreadyProcessed);
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Signature Verification Tests

    [Fact]
    public async Task ProcessAsync_InvalidSignature_ReturnsSignatureInvalid()
    {
        // Arrange
        var payload = "{}";
        var invalidSignature = "t=1234567890,v1=invalid_signature";

        // Act
        var result = await _handler.ProcessAsync(payload, invalidSignature, CancellationToken.None);

        // Assert
        result.Should().Be(WebhookProcessingResult.SignatureInvalid);
        await _orderRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Helper Methods

    private (string payload, string signature) CreateCheckoutSessionCompletedEvent()
    {
        // Create a properly structured event payload that Stripe's EventUtility can parse
        // For unit tests, we need to bypass signature verification by mocking at a higher level.
        // Since StripeWebhookHandler uses EventUtility.ConstructEvent internally,
        // we create a real signed event using the test webhook secret.
        var payload = CreateEventPayload("checkout.session.completed", CreateSessionObject());
        var signature = GenerateSignature(payload);
        return (payload, signature);
    }

    private (string payload, string signature) CreateCheckoutSessionExpiredEvent()
    {
        var payload = CreateEventPayload("checkout.session.expired", CreateSessionObject());
        var signature = GenerateSignature(payload);
        return (payload, signature);
    }

    private (string payload, string signature) CreateChargeRefundedEvent()
    {
        var chargeObject = $$"""
        {
            "id": "ch_test_123",
            "object": "charge",
            "payment_intent": "{{TestPaymentIntentId}}",
            "metadata": {
                "orderId": "{{TestOrderId}}",
                "eventId": "{{TestEventId}}"
            }
        }
        """;

        var payload = CreateEventPayload("charge.refunded", chargeObject);
        var signature = GenerateSignature(payload);
        return (payload, signature);
    }

    private string CreateSessionObject() => $$"""
        {
            "id": "{{TestSessionId}}",
            "object": "checkout.session",
            "payment_intent": "{{TestPaymentIntentId}}",
            "payment_status": "paid",
            "status": "complete",
            "metadata": {
                "orderId": "{{TestOrderId}}",
                "eventId": "{{TestEventId}}",
                "orderCode": "MDDD-TEST-0001"
            }
        }
        """;

    private static string CreateEventPayload(string eventType, string dataObject) => $$"""
        {
            "id": "{{TestStripeEventId}}",
            "object": "event",
            "type": "{{eventType}}",
            "api_version": "2026-04-22.dahlia",
            "data": {
                "object": {{dataObject}}
            }
        }
        """;

    private static string GenerateSignature(string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = EventUtility.ComputeSignature(TestWebhookSecret, timestamp, payload);
        return $"t={timestamp},v1={signature}";
    }

    #endregion
}
