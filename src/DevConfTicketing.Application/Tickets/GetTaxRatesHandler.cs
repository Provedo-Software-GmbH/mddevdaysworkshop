using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Application.Tickets;

public class GetTaxRatesHandler(ITaxRateRepository repository, ITelemetryService telemetry)
{
    public async Task<IReadOnlyList<TaxRate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetTaxRatesHandler)}.{nameof(GetAllAsync)}");
        try
        {
            return await repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<TaxRate>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetTaxRatesHandler)}.{nameof(GetByCountryCodeAsync)}");
        try
        {
            return await repository.GetByCountryCodeAsync(countryCode, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public async Task<TaxRate?> GetByIdAsync(string countryCode, string id, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(GetTaxRatesHandler)}.{nameof(GetByIdAsync)}");
        try
        {
            return await repository.GetByIdAsync(countryCode, id, cancellationToken);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }
}
