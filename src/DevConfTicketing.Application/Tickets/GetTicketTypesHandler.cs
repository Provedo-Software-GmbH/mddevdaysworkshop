using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class GetTicketTypesHandler(ITicketTypeRepository repository, ITelemetryService telemetry)
{
    public async Task<IReadOnlyList<TicketType>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetTicketTypesHandler)}.{nameof(GetByEventIdAsync)}");
        try
        {
            return await repository.GetByEventIdAsync(eventId, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public async Task<TicketType?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetTicketTypesHandler)}.{nameof(GetByIdAsync)}");
        try
        {
            return await repository.GetByIdAsync(eventId, id, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
