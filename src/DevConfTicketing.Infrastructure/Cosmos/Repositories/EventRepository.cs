using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

using Microsoft.Azure.Cosmos;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class EventRepository(CosmosDbService cosmosDbService) : IEventRepository
{
    private const string ContainerName = "events";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/id"
    };

    public async Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemAsync<Event>(ContainerName, id, id, cancellationToken);

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<Event>(ContainerName, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @status")
            .WithParameter("@status", status.ToString());

        return await cosmosDbService.QueryItemsAsync<Event>(ContainerName, query, cancellationToken);
    }

    public async Task<Event> CreateAsync(Event @event, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, @event, @event.Id, cancellationToken);

    public async Task<Event> UpdateAsync(Event @event, CancellationToken cancellationToken = default) =>
        await cosmosDbService.UpsertItemAsync(ContainerName, @event, @event.Id, cancellationToken);

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.DeleteItemAsync(ContainerName, id, id, cancellationToken);
}
