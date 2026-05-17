using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class CreateTaxRateHandler(ITaxRateRepository repository, ITelemetryService telemetry)
{
    public async Task<TaxRate> HandleAsync(TaxRate taxRate, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(CreateTaxRateHandler));
        try
        {
            var newTaxRate = new TaxRate
            {
                Id = Guid.NewGuid().ToString(),
                CountryCode = taxRate.CountryCode,
                Name = taxRate.Name,
                Percentage = taxRate.Percentage,
                Description = taxRate.Description,
                IsDefault = taxRate.IsDefault,
                IsActive = taxRate.IsActive
            };

            var created = await repository.CreateAsync(newTaxRate, cancellationToken);
            telemetry.IncrementCounter("taxrate.created");
            return created;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
