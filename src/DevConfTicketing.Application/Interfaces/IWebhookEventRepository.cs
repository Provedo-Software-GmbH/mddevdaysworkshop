using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Application.Interfaces;

public interface IWebhookEventRepository
{
    Task<bool> ExistsAsync(string eventId, string stripeEventId, CancellationToken cancellationToken = default);
    Task<StripeWebhookEvent> CreateAsync(StripeWebhookEvent webhookEvent, CancellationToken cancellationToken = default);
}
