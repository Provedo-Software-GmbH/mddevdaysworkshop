using System.Diagnostics;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Vouchers;

namespace DevConfTicketing.Application.Orders;

public class VoucherValidationService(IVoucherRepository repository, ITelemetryService telemetry)
{
    public async Task<Voucher> ValidateAsync(string eventId, string code, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan(nameof(VoucherValidationService));
        try
        {
            var voucher = await repository.GetByCodeAsync(eventId, code, cancellationToken)
                ?? throw new KeyNotFoundException($"Voucher with code '{code}' not found for event '{eventId}'.");

            if (!voucher.IsActive)
                throw new InvalidOperationException($"Voucher '{code}' is not active.");

            if (voucher.ValidUntil.HasValue && voucher.ValidUntil.Value < DateTimeOffset.UtcNow)
                throw new InvalidOperationException($"Voucher '{code}' has expired.");

            if (voucher.UsedCount >= voucher.MaxUsages)
                throw new InvalidOperationException($"Voucher '{code}' has reached its maximum usage limit.");

            telemetry.IncrementCounter("voucher.validated");
            return voucher;
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            throw;
        }
    }

    public decimal CalculateDiscount(Voucher voucher, decimal originalGross) =>
        voucher.DiscountType switch
        {
            DiscountType.Percentage => Math.Round(originalGross * voucher.DiscountValue / 100, 2),
            DiscountType.Absolute => Math.Min(voucher.DiscountValue, originalGross),
            DiscountType.FixedPrice => Math.Max(0, originalGross - voucher.DiscountValue),
            _ => 0m
        };
}
