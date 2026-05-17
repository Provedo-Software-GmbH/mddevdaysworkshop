using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Interfaces;

public interface ITicketTypeRepository
{
    Task<TicketType?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketType>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<TicketType> CreateAsync(TicketType ticketType, CancellationToken cancellationToken = default);
    Task<TicketType> UpdateAsync(TicketType ticketType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string eventId, string id, CancellationToken cancellationToken = default);
}
