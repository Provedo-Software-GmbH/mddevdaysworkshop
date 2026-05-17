using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Interfaces;

public interface ITaxRateRepository
{
    Task<TaxRate?> GetByIdAsync(string countryCode, string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRate>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<TaxRate> CreateAsync(TaxRate taxRate, CancellationToken cancellationToken = default);
    Task<TaxRate> UpdateAsync(TaxRate taxRate, CancellationToken cancellationToken = default);
}
