using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class UpdateTicketTypeHandler(ITicketTypeRepository repository, ITelemetryService telemetry)
{
    public async Task<TicketType> HandleAsync(string eventId, string id, TicketType updated, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(UpdateTicketTypeHandler));
        try
        {
            var existing = await repository.GetByIdAsync(eventId, id, cancellationToken)
                ?? throw new KeyNotFoundException($"TicketType with id '{id}' for event '{eventId}' was not found.");

            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Price = updated.Price;
            existing.Currency = updated.Currency;
            existing.AvailableQuantity = updated.AvailableQuantity;
            existing.SoldQuantity = updated.SoldQuantity;
            existing.MaxPerOrder = updated.MaxPerOrder;
            existing.ShowRemainingQuantity = updated.ShowRemainingQuantity;
            existing.SaleStart = updated.SaleStart;
            existing.SaleEnd = updated.SaleEnd;
            existing.LineItems = updated.LineItems;

            return await repository.UpdateAsync(existing, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
