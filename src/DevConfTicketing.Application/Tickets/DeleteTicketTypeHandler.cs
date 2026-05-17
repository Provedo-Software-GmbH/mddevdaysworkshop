using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class DeleteTicketTypeHandler(ITicketTypeRepository repository, ITelemetryService telemetry)
{
    public async Task HandleAsync(string eventId, string id, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(DeleteTicketTypeHandler));
        try
        {
            await repository.DeleteAsync(eventId, id, cancellationToken);
            telemetry.IncrementCounter("tickettype.deleted");
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
