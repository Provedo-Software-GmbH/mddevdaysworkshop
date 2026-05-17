using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class UpdateTaxRateHandler(ITaxRateRepository repository, ITelemetryService telemetry)
{
    public async Task<TaxRate> HandleAsync(string countryCode, string id, TaxRate taxRate, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(UpdateTaxRateHandler));
        try
        {
            var existing = await repository.GetByIdAsync(countryCode, id, cancellationToken)
                ?? throw new KeyNotFoundException($"TaxRate with id '{id}' and country code '{countryCode}' not found.");

            existing.Name = taxRate.Name;
            existing.Percentage = taxRate.Percentage;
            existing.Description = taxRate.Description;
            existing.IsDefault = taxRate.IsDefault;
            existing.IsActive = taxRate.IsActive;

            return await repository.UpdateAsync(existing, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
