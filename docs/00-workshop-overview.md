# Workshop Overview — KI-gestützte Entwicklung mit GitHub Copilot Agents

## Ziel

Ein ganztägiger Hands-on Workshop, in dem 6 erfahrene Entwickler\*innen lernen, agentenbasierte Entwicklung produktiv einzusetzen. Sie arbeiten an einem **realen Event-Management & Ticketing-System**, das anschließend produktiv genutzt wird.

## Zeitplan

| Zeit          | Block                                       |
| ------------- | ------------------------------------------- |
| 09:00 – 10:00 | Intro: Agentenbasierte Entwicklung, Mindset, Tooling |
| 10:00 – 11:00 | Demo: Repository-Walkthrough, Agent Setup, Model-Auswahl, erstes Issue |
| 11:00 – 11:15 | Pause                                       |
| 11:15 – 13:15 | Hands-on Block 1: Jede\*r arbeitet an eigenem Feature |
| 13:15 – 14:00 | Mittagspause                                |
| 14:00 – 16:00 | Hands-on Block 2: Feature fertigstellen, PRs reviewen |
| 16:00 – 16:30 | Wrap-up: Erfahrungen, Lessons Learned, Q&A  |

## Die Anwendung: **DevConf Ticketing**

Eine Event-Management-Plattform zum Erstellen von Entwicklerkonferenzen und zum Verkauf von Tickets mit differenzierter Besteuerung.

### Kernfeatures

- **Event-Management**: Events anlegen, bearbeiten, veröffentlichen
- **Ticket-Typen**: Verschiedene Ticket-Kategorien pro Event (Early Bird, Regular, VIP, etc.)
- **Positions-basierte Abrechnung**: Jedes Ticket kann mehrere Positionen enthalten (Übernachtung 7%, Beherbergungssteuer 0%, Verpflegungspauschale 19%, Konferenzticket 19%, etc.)
- **Stripe-Integration**: Bezahlung über Stripe Checkout
- **Gast-Checkout & Kundenkonten**: Kauf ohne Registrierung möglich, optional Konto via Entra External Identities
- **Admin-Backend**: Geschützt via Entra ID (App Registration)
- **Monitoring & KPIs**: OpenTelemetry with Azure Monitor exporter, Application Insights, Log Analytics

## Tech Stack

| Layer       | Technologie                                |
| ----------- | ------------------------------------------ |
| Backend     | .NET 11, Minimal APIs, Cosmos DB SDK       |
| Frontend    | React 19, TypeScript, Vite, shadcn/ui, Bun |
| Auth (Admin)| Microsoft Entra ID (App Registration)      |
| Auth (Customer) | Microsoft Entra External Identities    |
| Payments    | Stripe (Test Mode)                         |
| Monitoring  | OpenTelemetry SDK, Azure Monitor exporter, Application Insights, Log Analytics |
| Hosting     | Azure Container Apps (serverless)          |
| CI/CD       | GitHub Actions                             |
| Containers  | Dockerfiles (Backend: .NET, Frontend: nginx)|

## Was ist vorgebaut vs. was implementieren die Teilnehmer\*innen?

### ✅ Vorgebaut (Pre-implemented)

- Komplettes Projekt-Scaffolding (Backend + Frontend)
- Cosmos DB Repository Pattern & Setup
- Domain Models (Event, TicketType, Order, LineItem, TaxRate)
- Event CRUD API (Minimal APIs)
- TicketType CRUD API
- TaxRate CRUD API (Steuersätze verwalten)
- Frontend: Layout, Routing, shadcn/ui Setup
- Frontend: Event-Listing & Event-Detail Seiten
- Frontend: Admin-Layout mit Navigation
- Application Insights Integration (OpenTelemetry-based, using Activity & Metrics)
- Dockerfiles (Backend + Frontend)
- GitHub Actions CI Pipeline (Build + Test)
- Copilot Setup Steps, Custom Agents & Skills
- Alle GitHub Issues für die Teilnehmer\*innen

### 🔨 Von Teilnehmer\*innen implementiert (6 Features)

Jede\*r Teilnehmer\*in arbeitet an einem eigenständigen Feature:

1. **Stripe Payment, Refunds & Cancellations** — Checkout-Flow, Webhooks, Stornierung, Rückerstattung, Voucher-Einlösung
2. **Invoice, Tax Calculation & Cancellation Invoices** — Rechnungserstellung mit korrekter Steuerberechnung, Stornorechnungen
3. **Customer Auth & Account Management** — Entra External ID, Gastbestellung, Bestellhistorie
4. **Ticket PDF, QR-Codes & Email Notifications** — PDF-Tickets mit QR-Code, Download-Seite, Bestellbestätigung, Event-Erinnerungen
5. **Check-in & Ticket Scanning** — Check-in-Listen, QR-Scanner (Browser-Kamera), Teilnehmer-Suche, Check-in-Statistiken
6. **Admin Dashboard, KPIs & Data Export** — Verkaufszahlen, Auslastung, Echtzeit-Metriken, CSV-Export (Teilnehmer, Bestellungen, Steuer)

Siehe `docs/attendee-tasks/` für die detaillierten Aufgabenbeschreibungen.
