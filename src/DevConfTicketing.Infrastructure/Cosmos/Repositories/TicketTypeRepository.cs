using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class TicketTypeRepository(CosmosDbService cosmosDbService) : ITicketTypeRepository
{
    private const string ContainerName = "ticket-types";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/eventId"
    };

    public async Task<TicketType?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemAsync<TicketType>(ContainerName, id, eventId, cancellationToken);

    public async Task<IReadOnlyList<TicketType>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<TicketType>(ContainerName, eventId, cancellationToken);

    public async Task<TicketType> CreateAsync(TicketType ticketType, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, ticketType, ticketType.EventId, cancellationToken);

    public async Task<TicketType> UpdateAsync(TicketType ticketType, CancellationToken cancellationToken = default) =>
        await cosmosDbService.UpsertItemAsync(ContainerName, ticketType, ticketType.EventId, cancellationToken);

    public async Task DeleteAsync(string eventId, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.DeleteItemAsync(ContainerName, id, eventId, cancellationToken);
}
