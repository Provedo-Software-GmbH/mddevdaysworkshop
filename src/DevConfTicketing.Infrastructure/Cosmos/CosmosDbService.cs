using System.Diagnostics;
using System.Net;

using DevConfTicketing.Application.Interfaces;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace DevConfTicketing.Infrastructure.Cosmos;

public class CosmosDbService(CosmosClient cosmosClient, string databaseName, ITelemetryService telemetry, ILogger<CosmosDbService> logger)
{

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
        using var activity = StartCosmosSpan("GetItem", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            RecordSuccess("GetItem", containerName, stopwatch);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            RecordSuccess("GetItem", containerName, stopwatch);
            return null;
        }
        catch (Exception ex)
        {
            RecordFailure("GetItem", containerName, stopwatch, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<T>> GetItemsAsync<T>(string containerName, string? partitionKey = null, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = StartCosmosSpan("GetItems", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
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

            activity?.SetTag("db.cosmos.item_count", results.Count);
            RecordSuccess("GetItems", containerName, stopwatch);
            return results;
        }
        catch (Exception ex)
        {
            RecordFailure("GetItems", containerName, stopwatch, ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<T>> QueryItemsAsync<T>(string containerName, QueryDefinition queryDefinition, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = StartCosmosSpan("Query", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var query = container.GetItemQueryIterator<T>(queryDefinition);
            List<T> results = [];

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            activity?.SetTag("db.cosmos.item_count", results.Count);
            RecordSuccess("Query", containerName, stopwatch);
            return results;
        }
        catch (Exception ex)
        {
            RecordFailure("Query", containerName, stopwatch, ex);
            throw;
        }
    }

    public async Task<T> CreateItemAsync<T>(string containerName, T item, string partitionKey, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = StartCosmosSpan("CreateItem", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.CreateItemAsync(item, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            logger.LogDebug("Created item in {ContainerName} with partition key {PartitionKey}", containerName, partitionKey);
            RecordSuccess("CreateItem", containerName, stopwatch);
            return response.Resource;
        }
        catch (Exception ex)
        {
            RecordFailure("CreateItem", containerName, stopwatch, ex);
            throw;
        }
    }

    public async Task<T> UpsertItemAsync<T>(string containerName, T item, string partitionKey, CancellationToken cancellationToken = default) where T : class
    {
        using var activity = StartCosmosSpan("UpsertItem", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            var response = await container.UpsertItemAsync(item, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            logger.LogDebug("Upserted item in {ContainerName} with partition key {PartitionKey}", containerName, partitionKey);
            RecordSuccess("UpsertItem", containerName, stopwatch);
            return response.Resource;
        }
        catch (Exception ex)
        {
            RecordFailure("UpsertItem", containerName, stopwatch, ex);
            throw;
        }
    }

    public async Task DeleteItemAsync(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        using var activity = StartCosmosSpan("DeleteItem", containerName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerName);
            await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            logger.LogDebug("Deleted item {Id} from {ContainerName}", id, containerName);
            RecordSuccess("DeleteItem", containerName, stopwatch);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning("Attempted to delete non-existent item {Id} from {ContainerName}", id, containerName);
            RecordSuccess("DeleteItem", containerName, stopwatch);
        }
        catch (Exception ex)
        {
            RecordFailure("DeleteItem", containerName, stopwatch, ex);
            throw;
        }
    }

    private Activity? StartCosmosSpan(string operation, string containerName) =>
        telemetry.StartSpan($"CosmosDb.{operation}", ActivityKind.Client, new Dictionary<string, string>
        {
            ["db.system"] = "cosmosdb",
            ["db.name"] = databaseName,
            ["db.cosmos.container"] = containerName,
            ["db.operation"] = operation
        });

    private void RecordSuccess(string operation, string containerName, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        var tags = new Dictionary<string, string>
        {
            ["db.cosmos.container"] = containerName,
            ["db.operation"] = operation,
            ["db.status"] = "success"
        };

        telemetry.RecordHistogram("db.cosmos.duration", stopwatch.Elapsed.TotalMilliseconds, tags);
        telemetry.IncrementCounter("db.cosmos.operations", tags: tags);
    }

    private void RecordFailure(string operation, string containerName, Stopwatch stopwatch, Exception ex)
    {
        stopwatch.Stop();
        var tags = new Dictionary<string, string>
        {
            ["db.cosmos.container"] = containerName,
            ["db.operation"] = operation,
            ["db.status"] = "error"
        };

        telemetry.RecordHistogram("db.cosmos.duration", stopwatch.Elapsed.TotalMilliseconds, tags);
        telemetry.IncrementCounter("db.cosmos.operations", tags: tags);
        telemetry.TrackException(ex, tags);
    }
}
