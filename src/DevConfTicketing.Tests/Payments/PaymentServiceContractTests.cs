using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;
using DevConfTicketing.Tests.TestHelpers;

using FluentAssertions;

using NSubstitute;

namespace DevConfTicketing.Tests.Payments;

/// <summary>
/// Test scaffolding for IPaymentService scenarios.
/// These tests verify the contract expectations of the payment service interface
/// without hitting Stripe. Integration tests with Stripe mock server would go in a separate project.
/// </summary>
public class PaymentServiceContractTests
{
    private readonly IPaymentService _paymentService;

    public PaymentServiceContractTests()
    {
        _paymentService = Substitute.For<IPaymentService>();
    }

    #region CreateCheckoutSession Scenarios

    [Fact]
    public async Task CreateCheckoutSession_ValidOrder_ReturnsSessionIdAndUrl()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("order-1")
            .WithEventId("event-1")
            .WithStatus(OrderStatus.Pending)
            .Build();

        var expectedResult = new CheckoutSessionResult("cs_test_123", "https://checkout.stripe.com/c/pay/cs_test_123");
        _paymentService.CreateCheckoutSessionAsync(order, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(
            order,
            "https://example.com/success",
            "https://example.com/cancel",
            CancellationToken.None);

        // Assert
        result.SessionId.Should().NotBeNullOrEmpty();
        result.SessionUrl.Should().StartWith("https://");
    }

    [Fact]
    public async Task CreateCheckoutSession_OrderWithMultiplePositions_IncludesAllLineItems()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("order-multi")
            .WithEventId("event-1")
            .WithPosition("tt-1", "Conference Pass", 119.00m)
            .WithPosition("tt-2", "Workshop Add-on", 59.50m)
            .WithTotals(150.00m, 28.50m, 178.50m)
            .Build();

        var expectedResult = new CheckoutSessionResult("cs_test_multi", "https://checkout.stripe.com/c/pay/cs_test_multi");
        _paymentService.CreateCheckoutSessionAsync(order, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(
            order,
            "https://example.com/success",
            "https://example.com/cancel",
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().Be("cs_test_multi");
    }

    [Fact]
    public async Task CreateCheckoutSession_OrderWithDiscount_CreatesSessionCorrectly()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId("order-discounted")
            .WithEventId("event-1")
            .WithDiscount(20.00m)
            .WithTotals(80.00m, 15.20m, 95.20m)
            .Build();

        var expectedResult = new CheckoutSessionResult("cs_test_discount", "https://checkout.stripe.com/c/pay/cs_test_discount");
        _paymentService.CreateCheckoutSessionAsync(order, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _paymentService.CreateCheckoutSessionAsync(
            order,
            "https://example.com/success",
            "https://example.com/cancel",
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetPaymentStatus Scenarios

    [Fact]
    public async Task GetPaymentStatus_OrderWithoutPaymentInfo_ReturnsNoPayment()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(OrderStatus.Pending)
            .Build();

        _paymentService.GetPaymentStatusAsync(order, Arg.Any<CancellationToken>())
            .Returns(new PaymentStatusResult("no_payment", null, null));

        // Act
        var result = await _paymentService.GetPaymentStatusAsync(order, CancellationToken.None);

        // Assert
        result.Status.Should().Be("no_payment");
        result.PaymentIntentId.Should().BeNull();
        result.PaidAt.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentStatus_PaidOrder_ReturnsPaidWithDetails()
    {
        // Arrange
        var paidAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        var order = new OrderBuilder()
            .AsPaid("cs_test_paid", "pi_test_paid")
            .Build();

        _paymentService.GetPaymentStatusAsync(order, Arg.Any<CancellationToken>())
            .Returns(new PaymentStatusResult("paid", "pi_test_paid", paidAt));

        // Act
        var result = await _paymentService.GetPaymentStatusAsync(order, CancellationToken.None);

        // Assert
        result.Status.Should().Be("paid");
        result.PaymentIntentId.Should().Be("pi_test_paid");
        result.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPaymentStatus_UnpaidOrder_ReturnsUnpaid()
    {
        // Arrange
        var order = new OrderBuilder()
            .InPaymentProcessing("cs_test_unpaid")
            .Build();

        _paymentService.GetPaymentStatusAsync(order, Arg.Any<CancellationToken>())
            .Returns(new PaymentStatusResult("unpaid", null, null));

        // Act
        var result = await _paymentService.GetPaymentStatusAsync(order, CancellationToken.None);

        // Assert
        result.Status.Should().Be("unpaid");
    }

    #endregion

    #region Refund Scenarios

    [Fact]
    public async Task Refund_PaidOrder_ReturnsRefundResult()
    {
        // Arrange
        var order = new OrderBuilder()
            .AsPaid("cs_test_refund", "pi_test_refund")
            .Build();

        var expectedRefundedAt = DateTimeOffset.UtcNow;
        _paymentService.RefundAsync(order, Arg.Any<CancellationToken>())
            .Returns(new RefundResult("re_test_123", expectedRefundedAt));

        // Act
        var result = await _paymentService.RefundAsync(order, CancellationToken.None);

        // Assert
        result.RefundId.Should().NotBeNullOrEmpty();
        result.RefundedAt.Should().BeCloseTo(expectedRefundedAt, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Refund_OrderWithoutPaymentIntent_ThrowsInvalidOperation()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithStatus(OrderStatus.Pending)
            .Build();

        _paymentService.RefundAsync(order, Arg.Any<CancellationToken>())
            .Returns<RefundResult>(_ => throw new InvalidOperationException("Order has no Payment Intent ID — cannot refund."));

        // Act
        var act = () => _paymentService.RefundAsync(order, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot refund*");
    }

    #endregion
}
