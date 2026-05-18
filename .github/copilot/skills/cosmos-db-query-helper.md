# Cosmos DB Query Helper

## Description

Helps write efficient Cosmos DB queries, optimize RU consumption, and follow best practices for the DevConfTicketing application's data access patterns.

## Project Container Layout

| Container | Partition Key | Description |
|-----------|--------------|-------------|
| `events` | `/id` | Conference events (each event is its own partition) |
| `ticket-types` | `/eventId` | Ticket types grouped by event |
| `tax-rates` | `/countryCode` | Tax rates grouped by country |
| `orders` | `/eventId` | Orders grouped by event |
| `vouchers` | `/eventId` | Vouchers grouped by event |

## Point Reads vs Queries

### Point Reads (1 RU) — Always Prefer

A point read requires both the **id** and the **partition key value**. It costs exactly 1 RU for a 1KB document.

```csharp
// Point read — cheapest operation (1 RU)
var response = await container.ReadItemAsync<Event>(id, new PartitionKey(id));
```

Use point reads when:
- You know the exact `id` and partition key value
- Reading a single document

### Queries (Variable RU) — Use When Necessary

```csharp
// In-partition query — efficient, scans only one partition
var query = new QueryDefinition("SELECT * FROM c WHERE c.eventId = @eventId AND c.status = @status")
    .WithParameter("@eventId", eventId)
    .WithParameter("@status", "Active");

// IMPORTANT: Always specify the partition key in the query options
var options = new QueryRequestOptions { PartitionKey = new PartitionKey(eventId) };
```

## Avoiding Cross-Partition Queries

**Cross-partition queries are expensive** — they fan out to all partitions and consume significantly more RUs.

❌ **Bad** — cross-partition query:
```csharp
// This scans ALL partitions in the orders container
"SELECT * FROM c WHERE c.customerEmail = @email"
```

✅ **Good** — in-partition query:
```csharp
// This scans only the partition for the specific event
"SELECT * FROM c WHERE c.eventId = @eventId AND c.customerEmail = @email"
```

If you need to query by a field that isn't the partition key, consider:
1. **Change partition key** if the query pattern is common
2. **Denormalize data** — store a copy in a container with a different partition key
3. **Use change feed** to maintain a materialized view
4. **Accept the cross-partition cost** if the query is rare (e.g., admin reports)

## Query Best Practices

### Use Parameterized Queries
```csharp
// Always use parameters — never string interpolation
var query = new QueryDefinition("SELECT * FROM c WHERE c.title = @title")
    .WithParameter("@title", title);
```

### Project Only Needed Fields
```csharp
// Instead of SELECT * — reduces RU cost and network transfer
"SELECT c.id, c.title, c.status, c.startDate FROM c WHERE c.status = 'Published'"
```

### Use TOP for Pagination
```csharp
"SELECT TOP 20 * FROM c WHERE c.eventId = @eventId ORDER BY c.createdAt DESC"
```

### Use EXISTS for Checking Presence
```csharp
// Cheaper than reading the full document
"SELECT VALUE COUNT(1) FROM c WHERE c.id = @id"
```

### Avoid Functions in WHERE Clauses
```csharp
// ❌ Bad — can't use index
"SELECT * FROM c WHERE LOWER(c.title) = @title"

// ✅ Good — store lowercase version and query that
"SELECT * FROM c WHERE c.titleLower = @title"
```

## Indexing Policy Optimization

The default indexing policy indexes everything, which is good for flexibility but costs more RUs on writes.

### Recommended Policy for Events Container
```json
{
  "indexingMode": "consistent",
  "includedPaths": [
    { "path": "/status/?" },
    { "path": "/startDate/?" },
    { "path": "/endDate/?" },
    { "path": "/organizerId/?" }
  ],
  "excludedPaths": [
    { "path": "/description/?" },
    { "path": "/imageUrl/?" },
    { "path": "/websiteUrl/?" },
    { "path": "/*" }
  ]
}
```

### Recommended Policy for Orders Container
```json
{
  "indexingMode": "consistent",
  "includedPaths": [
    { "path": "/eventId/?" },
    { "path": "/status/?" },
    { "path": "/createdAt/?" },
    { "path": "/customerEmail/?" }
  ],
  "excludedPaths": [
    { "path": "/positions/*" },
    { "path": "/*" }
  ]
}
```

## RU Cost Estimation

| Operation | Approximate RU Cost |
|-----------|-------------------|
| Point read (1KB doc) | 1 RU |
| Point read (10KB doc) | ~3 RU |
| In-partition query (10 results) | ~3-10 RU |
| Cross-partition query | 10-100+ RU |
| Create document (1KB) | ~6 RU |
| Replace document (1KB) | ~11 RU |
| Delete document | ~6 RU |
| Upsert document (1KB) | ~11 RU |

## Repository Pattern in This Project

Repositories use the `CosmosDbService` for all operations:

```csharp
public class EventRepository(CosmosDbService cosmosDbService) : IEventRepository
{
    private static readonly CosmosContainerConfig Config = new("events", "/id");

    public async Task<Event?> GetByIdAsync(string id, CancellationToken ct = default)
        => await cosmosDbService.GetItem<Event>(Config, id, id, ct);

    public async Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct = default)
        => await cosmosDbService.GetItems<Event>(Config, ct);
}
```

Key patterns:
- `GetItem<T>(config, id, partitionKey)` — point read
- `GetItems<T>(config)` — get all items (use sparingly)
- `QueryItems<T>(config, query)` — parameterized query
- `Create<T>(config, item, partitionKey)` — create new document
- `Upsert<T>(config, item, partitionKey)` — create or replace
- `Delete(config, id, partitionKey)` — delete document

## Development vs Production

- **Development**: Uses Cosmos DB Emulator with a hardcoded emulator key
- **Production**: Uses `DefaultAzureCredential` (managed identity) — no keys in code
- Configured in `InfrastructureServiceRegistration.cs`

## Common Pitfalls

1. **Forgetting partition key on queries** — leads to expensive cross-partition scans
2. **Using SELECT * when you only need a few fields** — wastes RUs and bandwidth
3. **Not handling 429 (Too Many Requests)** — implement retry with exponential backoff (the SDK does this by default)
4. **Large documents** — keep documents under 100KB; split if larger
5. **Hot partitions** — ensure even data distribution across partition keys
6. **Not using the continuation token** for paginated queries
