using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Application.Events;

public class CreateEventHandler(IEventRepository repository, ITelemetryService telemetry)
{
    public async Task<Event> HandleAsync(Event @event, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(CreateEventHandler));
        try
        {
            var now = DateTimeOffset.UtcNow;
            var newEvent = new Event
            {
                Id = Guid.NewGuid().ToString(),
                Title = @event.Title,
                Description = @event.Description,
                Location = @event.Location,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                OrganizerId = @event.OrganizerId,
                Status = EventStatus.Draft,
                ImageUrl = @event.ImageUrl,
                WebsiteUrl = @event.WebsiteUrl,
                MaxAttendees = @event.MaxAttendees,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await repository.CreateAsync(newEvent, cancellationToken);
            telemetry.IncrementCounter("event.created");
            return created;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
