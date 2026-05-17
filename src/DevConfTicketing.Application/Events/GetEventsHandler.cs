using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Application.Events;

public class GetEventsHandler(IEventRepository repository, ITelemetryService telemetry)
{
    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetEventsHandler)}.{nameof(GetAllAsync)}");
        try
        {
            return await repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public async Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetEventsHandler)}.{nameof(GetByIdAsync)}");
        try
        {
            return await repository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
