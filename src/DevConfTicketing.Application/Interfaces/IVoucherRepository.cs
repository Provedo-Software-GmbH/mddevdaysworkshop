using DevConfTicketing.Domain.Vouchers;

namespace DevConfTicketing.Application.Interfaces;

public interface IVoucherRepository
{
    Task<Voucher?> GetByIdAsync(string eventId, string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Voucher>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<Voucher?> GetByCodeAsync(string eventId, string code, CancellationToken cancellationToken = default);
    Task<Voucher> CreateAsync(Voucher voucher, CancellationToken cancellationToken = default);
    Task<Voucher> UpdateAsync(Voucher voucher, CancellationToken cancellationToken = default);
    Task DeleteAsync(string eventId, string id, CancellationToken cancellationToken = default);
}
