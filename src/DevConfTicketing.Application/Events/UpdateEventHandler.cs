using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Application.Events;

public class UpdateEventHandler(IEventRepository repository, ITelemetryService telemetry)
{
    public async Task<Event> HandleAsync(string id, Event updated, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(UpdateEventHandler));
        try
        {
            var existing = await repository.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.Location = updated.Location;
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            existing.OrganizerId = updated.OrganizerId;
            existing.ImageUrl = updated.ImageUrl;
            existing.WebsiteUrl = updated.WebsiteUrl;
            existing.MaxAttendees = updated.MaxAttendees;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            return await repository.UpdateAsync(existing, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
