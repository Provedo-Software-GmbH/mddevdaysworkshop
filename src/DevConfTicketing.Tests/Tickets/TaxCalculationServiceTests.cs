using DevConfTicketing.Application.Tickets;
using DevConfTicketing.Domain.Orders;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Tests.Tickets;

public class TaxCalculationServiceTests
{
    private readonly TaxCalculationService _service = new(new Events.FakeTelemetryService());

    [Fact]
    public void CalculateLineItems_StandardRate19Percent_CalculatesCorrectTax()
    {
        var templates = new[]
        {
            new LineItemTemplate
            {
                Name = "Conference Ticket",
                NetAmount = 100.00m,
                TaxRateId = "tax-de-standard",
                TaxRateName = "MwSt. 19%",
                TaxRatePercentage = 19m
            }
        };

        var lineItems = _service.CalculateLineItems(templates);

        Assert.Single(lineItems);
        var item = lineItems[0];
        Assert.Equal("Conference Ticket", item.Name);
        Assert.Equal(100.00m, item.UnitNetAmount);
        Assert.Equal(19m, item.TaxRatePercentage);
        Assert.Equal(100.00m, item.TotalNet);
        Assert.Equal(19.00m, item.TaxAmount);
        Assert.Equal(119.00m, item.TotalGross);
    }

    [Fact]
    public void CalculateLineItems_ReducedRate7Percent_CalculatesCorrectTax()
    {
        var templates = new[]
        {
            new LineItemTemplate
            {
                Name = "Catering",
                NetAmount = 50.00m,
                TaxRateId = "tax-de-reduced",
                TaxRateName = "MwSt. 7%",
                TaxRatePercentage = 7m
            }
        };

        var lineItems = _service.CalculateLineItems(templates);

        var item = lineItems[0];
        Assert.Equal(50.00m, item.TotalNet);
        Assert.Equal(3.50m, item.TaxAmount);
        Assert.Equal(53.50m, item.TotalGross);
    }

    [Fact]
    public void CalculateLineItems_ZeroRate_NoTaxApplied()
    {
        var templates = new[]
        {
            new LineItemTemplate
            {
                Name = "Donation",
                NetAmount = 25.00m,
                TaxRateId = "tax-de-zero",
                TaxRateName = "Tax-free",
                TaxRatePercentage = 0m
            }
        };

        var lineItems = _service.CalculateLineItems(templates);

        var item = lineItems[0];
        Assert.Equal(25.00m, item.TotalNet);
        Assert.Equal(0m, item.TaxAmount);
        Assert.Equal(25.00m, item.TotalGross);
    }

    [Fact]
    public void CalculateLineItems_MultipleTemplates_AllConverted()
    {
        var templates = new[]
        {
            new LineItemTemplate
            {
                Name = "Ticket",
                NetAmount = 100.00m,
                TaxRateId = "tax-1",
                TaxRateName = "MwSt. 19%",
                TaxRatePercentage = 19m
            },
            new LineItemTemplate
            {
                Name = "Catering",
                NetAmount = 50.00m,
                TaxRateId = "tax-2",
                TaxRateName = "MwSt. 7%",
                TaxRatePercentage = 7m
            }
        };

        var lineItems = _service.CalculateLineItems(templates);

        Assert.Equal(2, lineItems.Count);
        Assert.Equal("Ticket", lineItems[0].Name);
        Assert.Equal("Catering", lineItems[1].Name);
    }

    [Fact]
    public void CalculateTotals_MultipleLineItemsMixedRates_SumsCorrectly()
    {
        var lineItems = new[]
        {
            new OrderLineItem
            {
                Name = "Conference Ticket",
                Quantity = 1,
                UnitNetAmount = 100.00m,
                TaxRatePercentage = 19m,
                TaxRateName = "MwSt. 19%"
            },
            new OrderLineItem
            {
                Name = "Catering Package",
                Quantity = 1,
                UnitNetAmount = 50.00m,
                TaxRatePercentage = 7m,
                TaxRateName = "MwSt. 7%"
            }
        };

        var (net, tax, gross) = _service.CalculateTotals(lineItems);

        Assert.Equal(150.00m, net);   // 100 + 50
        Assert.Equal(22.50m, tax);    // 19 + 3.50
        Assert.Equal(172.50m, gross); // 119 + 53.50
    }

    [Fact]
    public void CalculateTotals_EmptyLineItems_ReturnsZeros()
    {
        var (net, tax, gross) = _service.CalculateTotals([]);

        Assert.Equal(0m, net);
        Assert.Equal(0m, tax);
        Assert.Equal(0m, gross);
    }

    [Fact]
    public void CalculateTotals_RoundsToTwoDecimalPlaces()
    {
        // 33.33 net at 19% = 6.3327 tax → should round to 6.33
        var lineItems = new[]
        {
            new OrderLineItem
            {
                Name = "Item",
                Quantity = 1,
                UnitNetAmount = 33.33m,
                TaxRatePercentage = 19m,
                TaxRateName = "MwSt. 19%"
            }
        };

        var (net, tax, gross) = _service.CalculateTotals(lineItems);

        Assert.Equal(33.33m, net);
        Assert.Equal(6.33m, tax);
        Assert.Equal(39.66m, gross);
    }
}
