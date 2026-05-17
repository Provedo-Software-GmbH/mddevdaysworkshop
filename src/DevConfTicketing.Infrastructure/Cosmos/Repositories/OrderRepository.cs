using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class OrderRepository(CosmosDbService cosmosDbService) : IOrderRepository
{
    private const string ContainerName = "orders";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/eventId"
    };

    public async Task<Order?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemAsync<Order>(ContainerName, id, eventId, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<Order>(ContainerName, eventId, cancellationToken);

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, order, order.EventId, cancellationToken);

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default) =>
        await cosmosDbService.UpsertItemAsync(ContainerName, order, order.EventId, cancellationToken);
}
