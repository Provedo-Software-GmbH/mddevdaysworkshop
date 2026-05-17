using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class CreateTicketTypeHandler(ITicketTypeRepository repository, ITelemetryService telemetry)
{
    public async Task<TicketType> HandleAsync(string eventId, TicketType ticketType, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(CreateTicketTypeHandler), tags: new Dictionary<string, string>
        {
            ["eventId"] = eventId
        });
        try
        {
            var newTicketType = new TicketType
            {
                Id = Guid.NewGuid().ToString(),
                EventId = eventId,
                Name = ticketType.Name,
                Description = ticketType.Description,
                Price = ticketType.Price,
                Currency = ticketType.Currency,
                AvailableQuantity = ticketType.AvailableQuantity,
                SoldQuantity = ticketType.SoldQuantity,
                MaxPerOrder = ticketType.MaxPerOrder,
                ShowRemainingQuantity = ticketType.ShowRemainingQuantity,
                SaleStart = ticketType.SaleStart,
                SaleEnd = ticketType.SaleEnd,
                LineItems = ticketType.LineItems
            };

            var created = await repository.CreateAsync(newTicketType, cancellationToken);
            telemetry.IncrementCounter("tickettype.created");
            return created;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
