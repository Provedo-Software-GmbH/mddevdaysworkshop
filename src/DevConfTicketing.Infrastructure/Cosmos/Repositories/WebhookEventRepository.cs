using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class WebhookEventRepository(CosmosDbService cosmosDbService) : IWebhookEventRepository
{
    private const string ContainerName = "webhook-events";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/eventId"
    };

    public async Task<bool> ExistsAsync(string eventId, string stripeEventId, CancellationToken cancellationToken = default)
    {
        var existing = await cosmosDbService.GetItemAsync<StripeWebhookEvent>(ContainerName, stripeEventId, eventId, cancellationToken);
        return existing is not null;
    }

    public async Task<StripeWebhookEvent> CreateAsync(StripeWebhookEvent webhookEvent, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, webhookEvent, webhookEvent.EventId, cancellationToken);
}
