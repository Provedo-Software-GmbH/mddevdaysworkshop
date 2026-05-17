# Aufgabe 4: Ticket PDF, QR-Codes & Email Notifications

## Übersicht

Implementiere PDF-Ticket-Generierung mit QR-Codes für Check-in sowie E-Mail-Benachrichtigungen für Bestellbestätigungen, Ticket-Zustellung und Event-Erinnerungen. Nutze die **Microsoft Graph API** mit Application Permissions (`Mail.Send`) zum Versenden von E-Mails.

> **Inspiriert von pretix**: pretix hat ein komplettes Ticket-PDF-System mit individuellem QR-Code pro Position, Ticket-Download-Seiten und -APIs. Wir implementieren die Kernfeatures davon.

## Kontext

- Orders werden bereits erstellt und haben einen Status
- Keine E-Mail-Funktionalität ist bisher implementiert
- Application Insights ist für Monitoring bereits konfiguriert (via OpenTelemetry SDK mit Azure Monitor Exporter)
- Frontend OpenTelemetry ist vorkonfiguriert (`lib/telemetry.ts`): `fetch()` Calls senden automatisch `traceparent` Header an das Backend für End-to-End Distributed Tracing. Nutze `startSpan()` für Ticket-Download-Aktionen

## Aufgaben

### Backend

1. **Email Service** (`Infrastructure/Email/GraphEmailService.cs`):
   - Abstraktion: `IEmailService` Interface
   - Implementation mit **Microsoft Graph API** (`Microsoft.Graph` SDK):
     - Authentifizierung via **Managed Identity** (`DefaultAzureCredential`) — keine Client Secrets
     - `Mail.Send` Application Permission (direkt der Managed Identity zugewiesen, admin-consented)
     - Sendet über `graphClient.Users[senderEmail].SendMail.PostAsync(...)`
   - Sender: Shared Mailbox (z.B. `tickets@devconf-ticketing.de`)
   - Template-basierte E-Mails (HTML + Plain-Text)
   - Anhänge unterstützt (Ticket-PDFs)
   - Retry-Logik bei Fehler (mit Polly oder ähnlich)
   - Telemetry für gesendete/fehlgeschlagene E-Mails (via `ITelemetryService` — Activity-based tracing + Metrics)

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
   - Jede Order-Position hat ein kryptografisches `TicketSecret` (vorgebaut im Order-Model)
   - QR-Code kodiert das `TicketSecret` (nicht die Order-ID — Sicherheit!)
   - QR-Code als Base64 Image generieren (z.B. `QRCoder` NuGet Package)
   - In E-Mail und PDF einbetten

5. **PDF Ticket Generierung** (🆕 inspiriert von pretix):
   - PDF mit QuestPDF oder ähnlicher Library generieren
   - Jedes Ticket als eigene Seite im PDF:
     - Event-Name, Datum, Ort
     - Ticket-Typ und Teilnehmername
     - Bestellcode (z.B. `MDDD-A7K2`)
     - QR-Code (groß, scannbar)
     - Preisaufschlüsselung
   - Multi-Ticket PDF: alle Tickets einer Bestellung in einem PDF
   - Einzelticket-Download pro Position

6. **Ticket-Download Endpoints** (🆕):
   - `GET /api/v1/orders/{id}/tickets/pdf` — Alle Tickets als PDF
   - `GET /api/v1/orders/{id}/positions/{posId}/ticket/pdf` — Einzelticket PDF
   - `GET /api/v1/orders/{orderCode}/tickets/qr` — QR-Codes als JSON

7. **Notification Endpoints** (`Endpoints/NotificationEndpoints.cs`):
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
   - `/orders/{orderCode}/tickets` — Ticket-Seite mit QR-Code (🆕 per OrderCode, nicht ID)
   - "PDF herunterladen" Button (🆕)
   - Druckbare Version (CSS @media print)
   - Mobile-optimierte Ansicht
   - Einzelne Tickets auswählbar (bei Multi-Ticket Orders)

4. **E-Mail-Vorschau Komponente**:
   - Admin kann E-Mail-Template als Vorschau sehen
   - Platzhalter mit echten Daten befüllt

### Hinweise

- **Microsoft Graph API** mit `Mail.Send` Application Permission (kein delegierter Zugriff nötig)
- Graph SDK: `Microsoft.Graph` NuGet Package + `Azure.Identity` für `DefaultAzureCredential` (Managed Identity)
- Keine Client Secrets nötig — die Managed Identity hat die `Mail.Send` Application Permission direkt zugewiesen
- Sender-E-Mail-Adresse wird aus der Konfiguration geladen (Key Vault: `GraphApi--SenderEmail`)
- QR-Codes können mit einer einfachen Library generiert werden (z.B. `QRCoder` für .NET)
- E-Mails sollten HTML und Plain-Text Version haben
- Responsive E-Mail Templates (MJML oder inline CSS)
- E-Mail-Adressen validieren bevor gesendet wird
- Rate Limiting für Massenversand beachten (Graph API: 10.000 Emails/10min pro Mailbox)
- Ticket-PDFs als Anhang an Bestätigungs-E-Mail senden

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
- [ ] QR-Code wird für jedes Ticket generiert (basierend auf TicketSecret)
- [ ] 🆕 PDF-Tickets können heruntergeladen werden (alle + einzeln)
- [ ] 🆕 PDF enthält Event-Details, QR-Code, Teilnehmername, Bestellcode
- [ ] 🆕 Ticket-Download-Seite für Kunden (per OrderCode)
- [ ] Ticket-E-Mail enthält QR-Code und Link zur Download-Seite
- [ ] Event-Erinnerung kann an alle Teilnehmer gesendet werden
- [ ] Admin kann Benachrichtigungen verwalten und Vorschau sehen
- [ ] Mobile-freundliche Ticket-Ansicht mit QR-Code
- [ ] Unit Tests für PDF-Generierung, Template Rendering und QR-Code
- [ ] Fehlerbehandlung und Retry bei E-Mail-Versand
- [ ] Frontend-Telemetry: Custom Spans für Ticket-Aktionen (z.B. `ticket.downloadPdf`, `ticket.viewQrCode`) via `lib/telemetry.ts`
