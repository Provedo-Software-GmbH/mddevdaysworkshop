using System.Diagnostics;
using System.ComponentModel;
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

    public async Task<VoucherValidationResult> ValidateAsync(string eventId, string code, List<string>? ticketTypeIds, CancellationToken cancellationToken = default)
    {
        using var span = telemetry.StartSpan($"{nameof(VoucherValidationService)}.ValidateWithResult");
        try
        {
            var voucher = await repository.GetByCodeAsync(eventId, code, cancellationToken);
            if (voucher is null)
            {
                return VoucherValidationResult.Invalid($"Voucher with code '{code}' not found.");
            }

            if (!voucher.IsActive)
            {
                return VoucherValidationResult.Invalid($"Voucher '{code}' is not active.");
            }

            if (voucher.ValidUntil.HasValue && voucher.ValidUntil.Value < DateTimeOffset.UtcNow)
            {
                return VoucherValidationResult.Invalid($"Voucher '{code}' has expired.");
            }

            if (voucher.UsedCount >= voucher.MaxUsages)
            {
                return VoucherValidationResult.Invalid($"Voucher '{code}' has reached its maximum usage limit.");
            }

            if (ticketTypeIds is { Count: > 0 } && voucher.ApplicableTicketTypeIds is { Count: > 0 })
            {
                var hasApplicable = ticketTypeIds.Any(id => voucher.ApplicableTicketTypeIds.Contains(id));
                if (!hasApplicable)
                {
                    return VoucherValidationResult.Invalid($"Voucher '{code}' is not applicable to the selected ticket types.");
                }
            }

            telemetry.IncrementCounter("voucher.validated");
            return VoucherValidationResult.Valid(voucher.DiscountType, voucher.DiscountValue);
        }
        catch (Exception ex)
        {
            telemetry.TrackException(ex);
            return VoucherValidationResult.Invalid("An error occurred during voucher validation.");
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

[Description("Result of voucher validation containing validity status and discount information")]
public record VoucherValidationResult(bool IsValid, DiscountType? DiscountType, decimal? DiscountValue, string? ErrorMessage)
{
    public static VoucherValidationResult Valid(DiscountType discountType, decimal discountValue) =>
        new(true, discountType, discountValue, null);

    public static VoucherValidationResult Invalid(string errorMessage) =>
        new(false, null, null, errorMessage);
}
