# Aufgabe 4: Email Notifications & Ticket Delivery

## Übersicht

Implementiere E-Mail-Benachrichtigungen für Bestellbestätigungen, Ticket-Zustellung und Event-Erinnerungen. Nutze Azure Communication Services oder einen ähnlichen E-Mail-Provider.

## Kontext

- Orders werden bereits erstellt und haben einen Status
- Keine E-Mail-Funktionalität ist bisher implementiert
- Application Insights ist für Monitoring bereits konfiguriert

## Aufgaben

### Backend

1. **Email Service** (`Infrastructure/Email/EmailService.cs`):
   - Abstraktion: `IEmailService` Interface
   - Implementation mit Azure Communication Services (oder SendGrid als Alternative)
   - Template-basierte E-Mails
   - Retry-Logik bei Fehler
   - Telemetry für gesendete/fehlgeschlagene E-Mails

2. **Email Templates** (`Infrastructure/Email/EmailTemplates/`):
   - `OrderConfirmationTemplate` — Bestellbestätigung mit:
     - Bestellnummer, Datum
     - Event-Details (Name, Datum, Ort)
     - Ticketdetails und Preisaufschlüsselung
     - Zahlungsinformation
   - `TicketDeliveryTemplate` — Ticket-Zustellung mit:
     - QR-Code oder Barcode (für Check-in)
     - Ticket-Typ und Teilnehmername
     - Event-Details
   - `EventReminderTemplate` — Event-Erinnerung:
     - Event in X Tagen
     - Wichtige Informationen (Anfahrt, Check-in Zeit, etc.)
     - Link zum Event

3. **Notification Service** (`Application/Notifications/NotificationService.cs`):
   - `SendOrderConfirmation(orderId)` — Bestellbestätigung senden
   - `SendTicketDelivery(orderId)` — Tickets als E-Mail senden
   - `SendEventReminder(eventId)` — Erinnerung an alle Ticketinhaber
   - Queue-basiert oder direkt (für Workshop: direkt)

4. **QR-Code Generator**:
   - Eindeutiger Check-in Code pro Ticket
   - QR-Code als Base64 Image generieren
   - In E-Mail einbetten

5. **Notification Endpoints** (`Endpoints/NotificationEndpoints.cs`):
   - `POST /api/v1/orders/{id}/send-confirmation` — Bestellbestätigung senden
   - `POST /api/v1/orders/{id}/send-tickets` — Tickets senden
   - `POST /api/v1/events/{id}/send-reminder` — Erinnerung an alle senden
   - `GET /api/v1/notifications/templates` — Verfügbare Templates auflisten
   - `GET /api/v1/orders/{id}/tickets/qr` — QR-Codes für Order abrufen

6. **Automatische Benachrichtigungen**:
   - Nach erfolgreicher Zahlung (Webhook von Attendee 1) → Bestellbestätigung
   - Integration-Point: Event in `OrderStatusChanged` oder ähnlich

### Frontend

1. **Notification Management im Admin** (`/admin/notifications`):
   - Übersicht gesendeter Benachrichtigungen
   - "Erinnerung senden" Button pro Event
   - Template-Vorschau
   - Bestätigung vor Massenversand

2. **Order Detail erweitern** (Admin):
   - "Bestätigung erneut senden" Button
   - "Tickets senden" Button
   - Versand-Historie anzeigen

3. **Ticket-Ansicht für Kunden** (Public):
   - `/orders/{id}/tickets` — Ticket-Seite mit QR-Code
   - Druckbare Version
   - Mobile-optimierte Ansicht

4. **E-Mail-Vorschau Komponente**:
   - Admin kann E-Mail-Template als Vorschau sehen
   - Platzhalter mit echten Daten befüllt

### Hinweise

- Azure Communication Services hat ein kostenloses Kontingent
- QR-Codes können mit einer einfachen Library generiert werden (z.B. `QRCoder` für .NET)
- E-Mails sollten HTML und Plain-Text Version haben
- Responsive E-Mail Templates (MJML oder inline CSS)
- E-Mail-Adressen validieren bevor gesendet wird
- Rate Limiting für Massenversand beachten

### Agents verwenden

- Nutze den **backend-api-agent** für Endpoints und Services
- Nutze den **frontend-component-agent** für die Notification-UI
- Nutze den **testing-agent** für:
  - Template Rendering Tests
  - NotificationService Tests (mit gemocktem EmailService)
  - QR-Code Generierung Tests
- Nutze den **e2e-testing-agent** für einen Flow-Test (Order → Email Preview)

### Akzeptanzkriterien

- [ ] Bestellbestätigung wird per E-Mail gesendet
- [ ] E-Mails enthalten korrekte Bestelldetails und Preise
- [ ] QR-Code wird für jedes Ticket generiert
- [ ] Ticket-E-Mail enthält QR-Code
- [ ] Event-Erinnerung kann an alle Teilnehmer gesendet werden
- [ ] Admin kann Benachrichtigungen verwalten und Vorschau sehen
- [ ] Mobile-freundliche Ticket-Ansicht mit QR-Code
- [ ] Unit Tests für Template Rendering und QR-Code Generierung
- [ ] Fehlerbehandlung und Retry bei E-Mail-Versand
