using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default);
    Task<Event> CreateAsync(Event @event, CancellationToken cancellationToken = default);
    Task<Event> UpdateAsync(Event @event, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
