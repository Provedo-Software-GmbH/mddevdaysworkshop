# Stripe Payment Integration — Contract & Acceptance Criteria

## API Contracts

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| `POST` | `/api/v1/events/{eventId}/orders/{id}/checkout` | Create Stripe Checkout Session | Public |
| `GET` | `/api/v1/events/{eventId}/orders/{id}/payment-status` | Get current payment status | Public |
| `POST` | `/api/v1/events/{eventId}/orders/{id}/cancel` | Cancel order + Stripe refund | Admin |
| `POST` | `/api/v1/events/{eventId}/orders/{id}/refund` | Manual refund (admin) | Admin |
| `POST` | `/api/v1/webhooks/stripe` | Stripe webhook receiver | None (signature verified) |
| `POST` | `/api/v1/vouchers/validate` | Validate voucher code | Public |

## Domain Changes

- **`PaymentInfo`** added to `Order` — contains Stripe Session ID, Payment Intent ID, Charge ID, Refund ID, timestamps
- **`StripeWebhookEvent`** — idempotency store (partition key: `eventId`, unique: Stripe event ID)
- **`OrderStatus`** — already has: `Pending → PaymentProcessing → Paid → Cancelled / Refunded`
- **`CancellationDate`** — already on Order model

## Status Transitions

```
Pending ──→ PaymentProcessing ──→ Paid ──→ Cancelled
                                    │
                                    └──→ Refunded
PaymentProcessing ──→ Cancelled (session expired)
```

## Acceptance Criteria

### Checkout Flow
- [ ] `POST /orders/{id}/checkout` creates Stripe Checkout Session with correct line items
- [ ] Response contains `sessionUrl` for frontend redirect
- [ ] Order status transitions to `PaymentProcessing`
- [ ] Only `Pending` orders can start checkout (409 otherwise)

### Webhook Handling
- [ ] Stripe webhook signature is verified on every request
- [ ] `checkout.session.completed` → Order status = `Paid`, PaymentInfo updated
- [ ] `checkout.session.expired` → Order status = `Cancelled`
- [ ] `charge.refunded` → Order status = `Refunded`
- [ ] Duplicate events are idempotent (no double processing)
- [ ] Unknown event types return 200 (acknowledge without action)

### Cancellation & Refund
- [ ] `POST /orders/{id}/cancel` only works on `Paid` orders
- [ ] Cancel triggers Stripe refund automatically
- [ ] `CancellationDate` is set on the order
- [ ] Already checked-in tickets produce a warning in response (but don't block)
- [ ] `POST /orders/{id}/refund` works as admin-only manual refund
- [ ] Refund updates `PaymentInfo.StripeRefundId` and `RefundedAt`

### Voucher Validation
- [ ] `POST /vouchers/validate` validates code, expiry, usage limits
- [ ] Returns discount type and value on success
- [ ] Checks ticket type applicability when `ticketTypeIds` provided
- [ ] Returns 422 with descriptive error on failure

### Non-Functional
- [ ] No raw credit card data ever touches our system (Stripe Checkout only)
- [ ] All Stripe API calls use idempotency keys
- [ ] Webhook handler is idempotent via stored event IDs
- [ ] Telemetry spans wrap all payment operations
- [ ] Unit tests cover all status transitions and edge cases
- [ ] E2E test covers full checkout flow (mock Stripe)
