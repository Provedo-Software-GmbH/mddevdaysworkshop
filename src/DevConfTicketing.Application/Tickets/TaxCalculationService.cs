using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class TaxCalculationService(ITelemetryService telemetry)
{
    public List<OrderLineItem> CalculateLineItems(IEnumerable<LineItemTemplate> templates)
    {
        using var span = telemetry.StartSpan(nameof(TaxCalculationService));
        try
        {
            return templates.Select(t => new OrderLineItem
            {
                Name = t.Name,
                Quantity = 1,
                UnitNetAmount = t.NetAmount,
                TaxRatePercentage = t.TaxRatePercentage,
                TaxRateName = t.TaxRateName
            }).ToList();
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public (decimal Net, decimal Tax, decimal Gross) CalculateTotals(IEnumerable<OrderLineItem> lineItems)
    {
        using var span = telemetry.StartSpan($"{nameof(TaxCalculationService)}.{nameof(CalculateTotals)}");
        try
        {
            var net = lineItems.Sum(li => li.TotalNet);
            var tax = lineItems.Sum(li => li.TaxAmount);
            var gross = lineItems.Sum(li => li.TotalGross);
            return (Math.Round(net, 2), Math.Round(tax, 2), Math.Round(gross, 2));
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
