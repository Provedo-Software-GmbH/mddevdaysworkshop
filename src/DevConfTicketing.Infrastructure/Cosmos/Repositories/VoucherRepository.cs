using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Vouchers;

using Microsoft.Azure.Cosmos;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class VoucherRepository(CosmosDbService cosmosDbService) : IVoucherRepository
{
    private const string ContainerName = "vouchers";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/eventId"
    };

    public async Task<Voucher?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemAsync<Voucher>(ContainerName, id, eventId, cancellationToken);

    public async Task<IReadOnlyList<Voucher>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<Voucher>(ContainerName, eventId, cancellationToken);

    public async Task<Voucher?> GetByCodeAsync(string eventId, string code, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.code = @code")
            .WithParameter("@code", code);

        var results = await cosmosDbService.QueryItemsAsync<Voucher>(ContainerName, query, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<Voucher> CreateAsync(Voucher voucher, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, voucher, voucher.EventId, cancellationToken);

    public async Task<Voucher> UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default) =>
        await cosmosDbService.UpsertItemAsync(ContainerName, voucher, voucher.EventId, cancellationToken);

    public async Task DeleteAsync(string eventId, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.DeleteItemAsync(ContainerName, id, eventId, cancellationToken);
}
