using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Application.Tickets;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Application.Orders;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    ITicketTypeRepository ticketTypeRepository,
    IVoucherRepository voucherRepository,
    TaxCalculationService taxCalculationService,
    VoucherValidationService voucherValidationService,
    ITelemetryService telemetry)
{
    public async Task<Order> HandleAsync(
        string eventId,
        string customerEmail,
        string? customerName,
        Dictionary<string, int> ticketSelections,
        string? voucherCode = null,
        CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(CreateOrderHandler), tags: new Dictionary<string, string>
        {
            ["eventId"] = eventId
        });

        try
        {
            var positions = new List<OrderPosition>();
            var positionIndex = 0;
            var currency = "EUR";

            foreach (var (ticketTypeId, quantity) in ticketSelections)
            {
                var ticketType = await ticketTypeRepository.GetByIdAsync(eventId, ticketTypeId, cancellationToken)
                    ?? throw new KeyNotFoundException($"TicketType '{ticketTypeId}' not found for event '{eventId}'.");

                currency = ticketType.Currency;

                for (var i = 0; i < quantity; i++)
                {
                    var lineItems = taxCalculationService.CalculateLineItems(ticketType.LineItems);
                    var (net, tax, gross) = taxCalculationService.CalculateTotals(lineItems);

                    positions.Add(new OrderPosition
                    {
                        Index = positionIndex++,
                        TicketTypeId = ticketType.Id,
                        TicketTypeName = ticketType.Name,
                        TicketSecret = TicketSecretGenerator.Generate(),
                        LineItems = lineItems,
                        PositionNet = net,
                        PositionTax = tax,
                        PositionGross = gross
                    });
                }
            }

            var totalNet = positions.Sum(p => p.PositionNet);
            var totalTax = positions.Sum(p => p.PositionTax);
            var totalGross = positions.Sum(p => p.PositionGross);
            var discountAmount = 0m;

            if (!string.IsNullOrWhiteSpace(voucherCode))
            {
                var voucher = await voucherValidationService.ValidateAsync(eventId, voucherCode, cancellationToken);
                discountAmount = voucherValidationService.CalculateDiscount(voucher, totalGross);

                voucher.UsedCount++;
                await voucherRepository.UpdateAsync(voucher, cancellationToken);
            }

            var now = DateTimeOffset.UtcNow;
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                OrderCode = OrderCodeGenerator.Generate(),
                EventId = eventId,
                CustomerEmail = customerEmail,
                CustomerName = customerName,
                VoucherCode = voucherCode,
                Status = OrderStatus.Pending,
                Positions = positions,
                TotalNet = Math.Round(totalNet, 2),
                TotalTax = Math.Round(totalTax, 2),
                TotalGross = Math.Round(totalGross, 2),
                DiscountAmount = Math.Round(discountAmount, 2),
                Currency = currency,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await orderRepository.CreateAsync(order, cancellationToken);
            telemetry.IncrementCounter("order.created", tags: new Dictionary<string, string>
            {
                ["eventId"] = eventId
            });

            return created;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
