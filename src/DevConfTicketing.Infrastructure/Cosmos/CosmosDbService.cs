using System.Net;
using System.Text.Json;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace DevConfTicketing.Infrastructure.Cosmos;

public class CosmosDbService(CosmosClient cosmosClient, string databaseName, ILogger<CosmosDbService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task EnsureContainerAsync(CosmosContainerConfig config, CancellationToken cancellationToken = default)
    {
        var database = cosmosClient.GetDatabase(databaseName);
        await database.CreateContainerIfNotExistsAsync(
            config.ContainerName,
            config.PartitionKeyPath,
            cancellationToken: cancellationToken);

        logger.LogInformation("Ensured Cosmos DB container {ContainerName} exists", config.ContainerName);
    }

    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: cancellationToken);
        logger.LogInformation("Ensured Cosmos DB database {DatabaseName} exists", databaseName);
    }

    public async Task<T?> GetItemAsync<T>(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default) where T : class
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);

        try
        {
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<T>> GetItemsAsync<T>(string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : class
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);

        QueryRequestOptions? options = partitionKey is not null
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : null;

        var query = container.GetItemQueryIterator<T>(requestOptions: options);
        List<T> results = [];

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IReadOnlyList<T>> QueryItemsAsync<T>(string containerName, QueryDefinition queryDefinition, CancellationToken cancellationToken = default) where T : class
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);
        var query = container.GetItemQueryIterator<T>(queryDefinition);
        List<T> results = [];

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public async Task<T> CreateItemAsync<T>(string containerName, T item, string partitionKey, CancellationToken cancellationToken = default) where T : class
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);
        var response = await container.CreateItemAsync(item, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
        logger.LogDebug("Created item in {ContainerName} with partition key {PartitionKey}", containerName, partitionKey);
        return response.Resource;
    }

    public async Task<T> UpsertItemAsync<T>(string containerName, T item, string partitionKey, CancellationToken cancellationToken = default) where T : class
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);
        var response = await container.UpsertItemAsync(item, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
        logger.LogDebug("Upserted item in {ContainerName} with partition key {PartitionKey}", containerName, partitionKey);
        return response.Resource;
    }

    public async Task DeleteItemAsync(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        var container = cosmosClient.GetContainer(databaseName, containerName);

        try
        {
            await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            logger.LogDebug("Deleted item {Id} from {ContainerName}", id, containerName);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Attempted to delete non-existent item {Id} from {ContainerName}", id, containerName);
        }
    }
}
