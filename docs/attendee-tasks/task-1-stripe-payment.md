# Aufgabe 1: Stripe Payment Integration

## Übersicht

Implementiere die komplette Zahlungsabwicklung mit Stripe. Kund\*innen sollen über Stripe Checkout bezahlen können, und der Bestellstatus soll automatisch über Webhooks aktualisiert werden.

## Kontext

- Stripe Test-API-Keys werden als GitHub Secrets bereitgestellt (`STRIPE_SECRET_KEY`, `STRIPE_PUBLISHABLE_KEY`, `STRIPE_WEBHOOK_SECRET`)
- Im Backend existiert bereits ein Order-Model mit Status `Pending`
- Die Ticket-Auswahl im Frontend ist vorgebaut, aber der "Checkout" Button führt noch nirgendwo hin

## Aufgaben

### Backend

1. **Stripe NuGet Package hinzufügen**: `Stripe.net`
2. **Stripe Service erstellen** (`Infrastructure/Stripe/StripePaymentService.cs`):
   - Checkout Session erstellen mit:
     - Line Items aus der Order (mit korrekter Steuerberechnung)
     - Success/Cancel URLs
     - Kundeninformationen (Email)
     - Metadata (OrderId, EventId)
   - Payment Status abfragen
3. **Payment Endpoints** (`Endpoints/PaymentEndpoints.cs`):
   - `POST /api/v1/orders/{id}/checkout` — Erstellt Stripe Checkout Session, gibt Session-URL zurück
   - `POST /api/v1/webhooks/stripe` — Verarbeitet Stripe Webhooks
   - `GET /api/v1/orders/{id}/payment-status` — Gibt aktuellen Zahlungsstatus zurück
4. **Webhook Handler** (`Infrastructure/Stripe/StripeWebhookHandler.cs`):
   - Signature Verification
   - Handle `checkout.session.completed` → Order Status → `Paid`
   - Handle `checkout.session.expired` → Order Status → `Cancelled`
   - Idempotenz sicherstellen (Event-ID speichern)
5. **Order Status erweitern**:
   - `PaymentInfo` zum Order-Model hinzufügen (Stripe Session ID, Payment Intent ID)
   - Status-Übergänge validieren

### Frontend

1. **Checkout-Seite** (`/checkout`):
   - Bestellübersicht mit Positionen und Steueraufschlüsselung
   - Email-Eingabe für Gastbestellung
   - "Jetzt bezahlen" Button → ruft Backend auf → Redirect zu Stripe Checkout
2. **Erfolgsseite** (`/checkout/success`):
   - Bestellbestätigung mit Bestellnummer
   - Zusammenfassung der Bestellung
   - "Zurück zur Eventseite" Link
3. **Abbruchseite** (`/checkout/cancel`):
   - Hinweis dass Bezahlung abgebrochen wurde
   - "Erneut versuchen" und "Zurück" Buttons
4. **Checkout-Flow im Ticket-Selector verbinden**:
   - "Proceed to Checkout" → Order erstellen → Redirect zur Checkout-Seite

### Hinweise

- **Niemals** rohe Kreditkartendaten verarbeiten — nur Stripe Checkout verwenden
- Stripe Webhook Signature immer verifizieren
- Idempotency Keys für alle Stripe API Calls verwenden
- Im Test-Modus Stripe Test-Kreditkarten verwenden: `4242 4242 4242 4242`

### Agents verwenden

- Nutze den **backend-api-agent** für die Endpoint-Erstellung
- Nutze den **frontend-component-agent** für die React-Seiten
- Nutze den **testing-agent** um Unit Tests für den Payment Service zu erstellen
- Nutze den **e2e-testing-agent** für einen End-to-End Checkout-Flow Test
- Der **stripe-integration-helper** Skill enthält nützliche Patterns

### Akzeptanzkriterien

- [ ] Stripe Checkout Session wird korrekt erstellt mit allen Line Items
- [ ] Redirect zu Stripe Checkout funktioniert
- [ ] Webhook aktualisiert Order Status automatisch
- [ ] Success/Cancel Seiten zeigen korrekte Informationen
- [ ] Fehlerbehandlung für fehlgeschlagene Zahlungen
- [ ] Unit Tests für PaymentService
- [ ] E2E Test für den kompletten Checkout-Flow
