using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Application.Events;

public class PublishEventHandler(IEventRepository repository, ITelemetryService telemetry)
{
    public async Task<Event> HandleAsync(string id, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(PublishEventHandler));
        try
        {
            var @event = await repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            if (@event.Status is not EventStatus.Draft)
            {
                throw new InvalidOperationException(
                    $"Event '{id}' cannot be published because its status is '{@event.Status}'. Only Draft events can be published.");
            }

            @event.Status = EventStatus.Published;
            @event.UpdatedAt = DateTimeOffset.UtcNow;

            var updated = await repository.UpdateAsync(@event, cancellationToken);
            telemetry.IncrementCounter("event.published");
            return updated;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
