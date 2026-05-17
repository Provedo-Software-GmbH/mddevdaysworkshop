using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Infrastructure.Cosmos;

[Description("Configuration for a Cosmos DB container")]
public class CosmosContainerConfig
{
    [Description("Name of the Cosmos DB container")]
    [JsonPropertyName("containerName")]
    public required string ContainerName { get; init; }

    [Description("Partition key path for the container")]
    [JsonPropertyName("partitionKeyPath")]
    public required string PartitionKeyPath { get; init; }
}
