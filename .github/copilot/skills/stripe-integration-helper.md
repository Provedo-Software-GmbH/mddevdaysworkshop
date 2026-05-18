# Stripe Integration Helper

## Description

Provides guidance on Stripe API integration patterns for the DevConfTicketing application, covering Checkout Sessions, webhook handling, and PCI compliance.

## Stripe.NET SDK

The project uses the **Stripe.NET** NuGet package (`Stripe.net`) for server-side integration. Never handle raw card data — always use Stripe Checkout or Stripe Elements.

## Creating a Checkout Session

When an order is placed, create a Stripe Checkout Session to redirect the user to Stripe's hosted payment page:

```csharp
using Stripe;
using Stripe.Checkout;

public async Task<string> CreateCheckoutSessionAsync(Order order, string successUrl, string cancelUrl)
{
    var options = new SessionCreateOptions
    {
        PaymentMethodTypes = ["card"],
        Mode = "payment",
        SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
        CancelUrl = cancelUrl,
        ClientReferenceId = order.Id,
        CustomerEmail = order.CustomerEmail,
        LineItems = order.Positions.Select(p => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = order.Currency.ToLowerInvariant(),
                UnitAmount = (long)(p.GrossAmount * 100), // Stripe uses cents
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = p.TicketTypeName,
                    Description = $"Ticket for {order.EventTitle}"
                }
            },
            Quantity = 1
        }).ToList(),
        Metadata = new Dictionary<string, string>
        {
            ["orderId"] = order.Id,
            ["eventId"] = order.EventId
        }
    };

    var service = new SessionService();
    var session = await service.CreateAsync(options);
    return session.Url; // Redirect user here
}
```

## Webhook Handling

Stripe sends webhook events for payment lifecycle. Always verify the webhook signature:

```csharp
app.MapPost("/api/v1/webhooks/stripe", async (HttpRequest request, IConfiguration config) =>
{
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var endpointSecret = config["Stripe:WebhookSecret"];

    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            request.Headers["Stripe-Signature"],
            endpointSecret
        );

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                var session = stripeEvent.Data.Object as Session;
                // Mark order as paid, generate tickets
                break;

            case EventTypes.PaymentIntentPaymentFailed:
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                // Mark order as failed, notify customer
                break;

            case EventTypes.ChargeRefunded:
                var charge = stripeEvent.Data.Object as Charge;
                // Process refund, cancel tickets
                break;
        }

        return Results.Ok();
    }
    catch (StripeException)
    {
        return Results.BadRequest();
    }
});
```

## Idempotency Keys

For any operation that creates or modifies resources, use idempotency keys to prevent duplicate charges:

```csharp
var options = new SessionCreateOptions { /* ... */ };
var requestOptions = new RequestOptions
{
    IdempotencyKey = $"order-{order.Id}-checkout"
};

var session = await service.CreateAsync(options, requestOptions);
```

Rules for idempotency keys:
- Use a **deterministic key** based on the business operation (e.g., `order-{orderId}-checkout`)
- Keys are scoped to the API key and expire after 24 hours
- If a request with the same key is retried, Stripe returns the original response

## Error Handling

```csharp
try
{
    var session = await service.CreateAsync(options);
}
catch (StripeException ex)
{
    // ex.StripeError.Type — error type (api_error, card_error, etc.)
    // ex.StripeError.Code — specific error code
    // ex.StripeError.Message — human-readable message
    // ex.HttpStatusCode — HTTP status from Stripe

    return ex.StripeError.Type switch
    {
        "card_error" => Results.UnprocessableEntity(new { error = ex.StripeError.Message }),
        "rate_limit_error" => Results.StatusCode(429),
        "api_error" => Results.StatusCode(502),
        _ => Results.StatusCode(500)
    };
}
```

## Test Mode vs Live Mode

- **Test mode**: Use API keys starting with `sk_test_` and `pk_test_`
- **Live mode**: Use API keys starting with `sk_live_` and `pk_live_`
- Test card numbers:
  - `4242 4242 4242 4242` — Visa, always succeeds
  - `4000 0000 0000 0002` — always declines
  - `4000 0000 0000 3220` — triggers 3D Secure
- Store API keys in **GitHub Secrets** and inject via environment variables — never commit keys to source code

## Configuration

In `appsettings.json` (values come from environment variables or Azure Key Vault):

```json
{
  "Stripe": {
    "SecretKey": "", // sk_test_... or sk_live_...
    "PublishableKey": "", // pk_test_... or pk_live_...
    "WebhookSecret": "" // whsec_...
  }
}
```

In `Program.cs`:
```csharp
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
```

## PCI Compliance

**CRITICAL**: The DevConfTicketing application must never handle raw card data.

- ✅ Use **Stripe Checkout** (hosted payment page) — simplest PCI compliance (SAQ A)
- ✅ Use **Stripe Elements** (embedded UI components) — PCI compliance with SAQ A-EP
- ❌ Never collect card numbers, CVVs, or expiration dates in our forms
- ❌ Never log or store payment card information
- ❌ Never send card data to our backend

## Frontend Integration

On the frontend, redirect to the Stripe Checkout URL returned by the backend:

```typescript
const createCheckoutSession = async (orderId: string): Promise<string> => {
  const response = await fetch(`/api/v1/orders/${orderId}/checkout`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  });
  const { checkoutUrl } = await response.json();
  return checkoutUrl;
};

// Redirect to Stripe
window.location.href = await createCheckoutSession(orderId);
```

## Order Lifecycle with Stripe

1. **Order Created** → `OrderStatus.Pending` — order saved in Cosmos DB
2. **Checkout Session Created** → user redirected to Stripe
3. **Payment Succeeds** → webhook `checkout.session.completed` → `OrderStatus.Paid`
4. **Payment Fails** → webhook `payment_intent.payment_failed` → `OrderStatus.PaymentFailed`
5. **Refund Requested** → admin action → Stripe refund API → `OrderStatus.Refunded`
6. **Refund Completed** → webhook `charge.refunded` → update order

## Telemetry

Track Stripe operations using the project's telemetry service:
- `stripe.checkout.created` — counter for checkout sessions created
- `stripe.payment.succeeded` — counter for successful payments
- `stripe.payment.failed` — counter for failed payments with reason tag
- `stripe.webhook.received` — counter for webhook events by type
- `stripe.api.duration` — histogram of Stripe API call durations
