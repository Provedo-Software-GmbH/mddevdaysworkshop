using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Tests.Orders;

public class OrderTotalCalculationTests
{
    [Fact]
    public void OrderLineItem_SingleQuantity_19PercentTax()
    {
        var lineItem = new OrderLineItem
        {
            Name = "Conference Ticket",
            Quantity = 1,
            UnitNetAmount = 100.00m,
            TaxRatePercentage = 19m,
            TaxRateName = "MwSt. 19%"
        };

        Assert.Equal(100.00m, lineItem.TotalNet);
        Assert.Equal(19.00m, lineItem.TaxAmount);
        Assert.Equal(119.00m, lineItem.TotalGross);
    }

    [Fact]
    public void OrderLineItem_MultipleQuantity_MultipliesCorrectly()
    {
        var lineItem = new OrderLineItem
        {
            Name = "Conference Ticket",
            Quantity = 3,
            UnitNetAmount = 100.00m,
            TaxRatePercentage = 19m,
            TaxRateName = "MwSt. 19%"
        };

        Assert.Equal(300.00m, lineItem.TotalNet);
        Assert.Equal(57.00m, lineItem.TaxAmount);
        Assert.Equal(357.00m, lineItem.TotalGross);
    }

    [Fact]
    public void OrderLineItem_ReducedRate7Percent()
    {
        var lineItem = new OrderLineItem
        {
            Name = "Catering",
            Quantity = 1,
            UnitNetAmount = 50.00m,
            TaxRatePercentage = 7m,
            TaxRateName = "MwSt. 7%"
        };

        Assert.Equal(50.00m, lineItem.TotalNet);
        Assert.Equal(3.50m, lineItem.TaxAmount);
        Assert.Equal(53.50m, lineItem.TotalGross);
    }

    [Fact]
    public void OrderLineItem_ZeroTaxRate()
    {
        var lineItem = new OrderLineItem
        {
            Name = "Donation",
            Quantity = 1,
            UnitNetAmount = 25.00m,
            TaxRatePercentage = 0m,
            TaxRateName = "Tax-free"
        };

        Assert.Equal(25.00m, lineItem.TotalNet);
        Assert.Equal(0m, lineItem.TaxAmount);
        Assert.Equal(25.00m, lineItem.TotalGross);
    }

    [Fact]
    public void OrderPosition_SumsLineItemsCorrectly()
    {
        var position = new OrderPosition
        {
            Index = 0,
            TicketTypeId = "tt-1",
            TicketTypeName = "Full Pass",
            TicketSecret = "secret-123",
            LineItems =
            [
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
                    Name = "Catering",
                    Quantity = 1,
                    UnitNetAmount = 50.00m,
                    TaxRatePercentage = 7m,
                    TaxRateName = "MwSt. 7%"
                }
            ],
            PositionNet = 150.00m,
            PositionTax = 22.50m,
            PositionGross = 172.50m
        };

        var expectedNet = position.LineItems.Sum(li => li.TotalNet);
        var expectedTax = position.LineItems.Sum(li => li.TaxAmount);
        var expectedGross = position.LineItems.Sum(li => li.TotalGross);

        Assert.Equal(expectedNet, position.PositionNet);
        Assert.Equal(expectedTax, position.PositionTax);
        Assert.Equal(expectedGross, position.PositionGross);
    }

    [Fact]
    public void Order_MultiplePositions_TotalsAreCorrect()
    {
        var order = new Order
        {
            Id = "order-1",
            OrderCode = "MDDD-0001",
            EventId = "event-1",
            CustomerEmail = "test@example.com",
            Positions =
            [
                new OrderPosition
                {
                    Index = 0,
                    TicketTypeId = "tt-1",
                    TicketTypeName = "Full Pass",
                    TicketSecret = "secret-1",
                    LineItems =
                    [
                        new OrderLineItem
                        {
                            Name = "Conference",
                            Quantity = 1,
                            UnitNetAmount = 100.00m,
                            TaxRatePercentage = 19m,
                            TaxRateName = "MwSt. 19%"
                        }
                    ],
                    PositionNet = 100.00m,
                    PositionTax = 19.00m,
                    PositionGross = 119.00m
                },
                new OrderPosition
                {
                    Index = 1,
                    TicketTypeId = "tt-1",
                    TicketTypeName = "Full Pass",
                    TicketSecret = "secret-2",
                    LineItems =
                    [
                        new OrderLineItem
                        {
                            Name = "Conference",
                            Quantity = 1,
                            UnitNetAmount = 100.00m,
                            TaxRatePercentage = 19m,
                            TaxRateName = "MwSt. 19%"
                        }
                    ],
                    PositionNet = 100.00m,
                    PositionTax = 19.00m,
                    PositionGross = 119.00m
                }
            ],
            TotalNet = 200.00m,
            TotalTax = 38.00m,
            TotalGross = 238.00m,
            DiscountAmount = 0m,
            Currency = "EUR"
        };

        var expectedNet = order.Positions.Sum(p => p.PositionNet);
        var expectedTax = order.Positions.Sum(p => p.PositionTax);
        var expectedGross = order.Positions.Sum(p => p.PositionGross);

        Assert.Equal(expectedNet, order.TotalNet);
        Assert.Equal(expectedTax, order.TotalTax);
        Assert.Equal(expectedGross, order.TotalGross);
        Assert.Equal(0m, order.DiscountAmount);
    }

    [Fact]
    public void OrderLineItem_FractionalAmount_ComputedPropertiesWork()
    {
        // Test with a price that creates rounding scenarios
        var lineItem = new OrderLineItem
        {
            Name = "Workshop Add-on",
            Quantity = 2,
            UnitNetAmount = 33.33m,
            TaxRatePercentage = 19m,
            TaxRateName = "MwSt. 19%"
        };

        Assert.Equal(66.66m, lineItem.TotalNet);       // 33.33 * 2
        Assert.Equal(12.6654m, lineItem.TaxAmount);     // 66.66 * 0.19
        Assert.Equal(79.3254m, lineItem.TotalGross);    // 66.66 + 12.6654
    }
}
