using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
