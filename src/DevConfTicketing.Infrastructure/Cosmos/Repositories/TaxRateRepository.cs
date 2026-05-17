using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Infrastructure.Cosmos.Repositories;

public class TaxRateRepository(CosmosDbService cosmosDbService) : ITaxRateRepository
{
    private const string ContainerName = "tax-rates";

    public static readonly CosmosContainerConfig ContainerConfig = new()
    {
        ContainerName = ContainerName,
        PartitionKeyPath = "/countryCode"
    };

    public async Task<TaxRate?> GetByIdAsync(string countryCode, string id, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemAsync<TaxRate>(ContainerName, id, countryCode, cancellationToken);

    public async Task<IReadOnlyList<TaxRate>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<TaxRate>(ContainerName, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<TaxRate>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default) =>
        await cosmosDbService.GetItemsAsync<TaxRate>(ContainerName, countryCode, cancellationToken);

    public async Task<TaxRate> CreateAsync(TaxRate taxRate, CancellationToken cancellationToken = default) =>
        await cosmosDbService.CreateItemAsync(ContainerName, taxRate, taxRate.CountryCode, cancellationToken);

    public async Task<TaxRate> UpdateAsync(TaxRate taxRate, CancellationToken cancellationToken = default) =>
        await cosmosDbService.UpsertItemAsync(ContainerName, taxRate, taxRate.CountryCode, cancellationToken);
}
