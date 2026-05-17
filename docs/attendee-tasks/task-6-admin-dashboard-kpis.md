# Aufgabe 6: Admin Dashboard, KPIs & Data Export

## Übersicht

Implementiere ein Admin-Dashboard mit Echtzeit-Verkaufszahlen, Auslastungsmetriken und Business-KPIs. Zusätzlich: CSV/Excel-Export von Teilnehmerlisten und Bestellungen für die Buchhaltung. Nutze Application Insights Custom Metrics für das Backend und Recharts/shadcn Charts für das Frontend.

> **Inspiriert von pretix**: pretix bietet umfangreiche Exporter (CSV/XLSX für Orders, Teilnehmer, Steuerberichte, Check-in-Listen) und ein Statistics-Dashboard. Wir implementieren die wichtigsten Exporte + Dashboard.

## Kontext

- Application Insights ist bereits im Backend integriert
- Der `TelemetryService` existiert mit grundlegenden Custom Events
- shadcn/ui `chart` Komponente (Recharts-basiert) ist vorinstalliert
- Admin-Layout mit Sidebar ist vorgebaut

## Aufgaben

### Backend

1. **Metrics Domain** (`Domain/Metrics/`):
   - `SalesMetric.cs` — Verkaufsdaten
   - `RevenueMetric.cs` — Umsatzdaten
   - `EventMetric.cs` — Event-spezifische Metriken

2. **Metrics Service** (`Application/Dashboard/MetricsService.cs`):
   - **Verkaufs-KPIs**:
     - Verkaufte Tickets (gesamt, pro Event, pro Ticket-Typ)
     - Umsatz (Brutto, Netto, Steuer — pro Steuersatz)
     - Durchschnittlicher Bestellwert
     - Conversion Rate (Besucher → Kauf)
   - **Event-KPIs**:
     - Auslastung pro Event (verkauft / verfügbar)
     - Auslastung pro Ticket-Typ
     - Beliebteste Events
     - Verkaufstrend über Zeit
   - **Operative KPIs**:
     - Offene Bestellungen (Pending)
     - Stornierungsrate
     - Durchschnittliche Bearbeitungszeit

3. **Dashboard Endpoints** (`Endpoints/DashboardEndpoints.cs`):
   - `GET /api/v1/dashboard/overview` — Gesamtübersicht (alle Events)
   - `GET /api/v1/dashboard/events/{eventId}/sales` — Verkaufszahlen pro Event
   - `GET /api/v1/dashboard/events/{eventId}/revenue` — Umsatz pro Event
   - `GET /api/v1/dashboard/events/{eventId}/occupancy` — Auslastung
   - `GET /api/v1/dashboard/events/{eventId}/trend` — Verkaufstrend (Zeitreihe)
   - `GET /api/v1/dashboard/tax-summary` — Steuer-Übersicht für Buchhaltung

4. **Application Insights Custom Metrics erweitern**:
   - `TicketSold` Metrik mit Dimensionen (EventId, TicketTypeId, Amount)
   - `OrderCreated` Metrik mit Dimensionen (EventId, PaymentMethod, Amount)
   - `CheckoutStarted` / `CheckoutCompleted` für Conversion Tracking
   - `PageViewed` Custom Event (EventId, Page)

5. **Aggregation**:
   - Cosmos DB Queries für aggregierte Daten
   - Optional: Materialized View Pattern (vorberechnete Aggregate)
   - Zeitraum-Filter: Heute, Diese Woche, Dieser Monat, Custom Range

6. **Data Export Endpoints** (🆕 inspiriert von pretix):
   - `GET /api/v1/events/{eventId}/export/attendees?format=csv` — Teilnehmerliste
     - Spalten: Name, Email, Ticket-Typ, Bestellcode, Bestelldatum, Check-in-Status
   - `GET /api/v1/events/{eventId}/export/orders?format=csv` — Bestellübersicht
     - Spalten: Bestellcode, Datum, Kunde, Status, Netto, Steuer, Brutto, Zahlungsmethode
   - `GET /api/v1/events/{eventId}/export/tax-report?format=csv` — Steuerbericht
     - Spalten: Steuersatz, Nettobetrag, Steuerbetrag, Bruttobetrag (gruppiert)
   - Content-Type: `text/csv` mit UTF-8 BOM (für Excel-Kompatibilität)
   - Optional: `format=xlsx` als Stretch Goal

### Frontend

1. **Dashboard Übersicht** (`/admin/dashboard`):
   - **KPI Cards** (oben):
     - Gesamtumsatz (Brutto)
     - Verkaufte Tickets (Anzahl)
     - Aktive Events (Anzahl)
     - Durchschnittlicher Bestellwert
   - **Verkaufstrend Chart** (Mitte):
     - Line Chart: Verkäufe über Zeit
     - Filterbar: 7 Tage, 30 Tage, 90 Tage, Custom
   - **Event-Übersicht** (unten):
     - Bar Chart: Auslastung pro Event
     - Tabelle: Top Events nach Umsatz

2. **Event Dashboard** (`/admin/events/:id/dashboard`):
   - KPI Cards: Verkaufte Tickets, Umsatz, Auslastung, Stornierungsrate
   - Pie Chart: Verteilung nach Ticket-Typ
   - Line Chart: Verkaufstrend für dieses Event
   - Tabelle: Letzte Bestellungen

3. **Steuer-Dashboard** (`/admin/dashboard/tax`):
   - Steuer-Zusammenfassung pro Steuersatz
   - Exportierbar als CSV (für Buchhaltung)
   - Zeitraum-Filter

4. **Export-Bereich** (🆕 inspiriert von pretix) (`/admin/events/:id/export`):
   - Übersicht aller verfügbaren Exporte
   - "Teilnehmerliste exportieren" → CSV Download
   - "Bestellungen exportieren" → CSV Download
   - "Steuerbericht exportieren" → CSV Download
   - Zeitraum-Filter für alle Exporte
   - Status-Filter (nur bezahlte, alle, stornierte)

5. **Dashboard Komponenten**:
   - `KpiCard` — Zahl mit Trend-Indikator (↑/↓)
   - `SalesTrendChart` — Line/Area Chart mit Zeitfilter
   - `OccupancyChart` — Horizontal Bar Chart
   - `RevenuePieChart` — Pie/Donut Chart
   - `TaxSummaryTable` — Tabelle mit Steuer-Zusammenfassung
   - `RecentOrdersTable` — Letzte Bestellungen
   - `DateRangePicker` — Zeitraum-Auswahl

5. **Echtzeit-Updates** (Optional, Stretch Goal):
   - Polling alle 30 Sekunden für Dashboard-Daten
   - Oder: SignalR für Push-Updates

### Hinweise

- Cosmos DB unterstützt Aggregationen, aber sie kosten RUs — effizient abfragen
- Für das Workshop-Setting reicht Polling; SignalR ist ein Stretch Goal
- shadcn/ui Charts basieren auf Recharts — Dokumentation beachten
- Responsive Charts für verschiedene Bildschirmgrößen
- Leere Zustände berücksichtigen (keine Daten → hilfreiche Nachricht)

### Agents verwenden

- Nutze den **backend-api-agent** für Dashboard Endpoints
- Nutze den **frontend-component-agent** für Charts und Dashboard
- Nutze den **kpi-agent** — dieser Agent kann helfen, neue KPIs zu identifizieren
- Nutze den **testing-agent** für:
  - MetricsService Berechnungs-Tests
  - Aggregation Tests
  - Endpoint Response Format Tests
- Nutze den **e2e-testing-agent** für Dashboard-Darstellung

### Akzeptanzkriterien

- [ ] Dashboard-Übersicht zeigt alle KPIs korrekt
- [ ] Verkaufstrend als Chart dargestellt
- [ ] Auslastung pro Event sichtbar
- [ ] Event-spezifisches Dashboard funktioniert
- [ ] Steuer-Zusammenfassung für Buchhaltung verfügbar
- [ ] Zeitraum-Filter funktioniert
- [ ] Responsive Design für alle Charts
- [ ] Application Insights Custom Metrics werden emittiert
- [ ] 🆕 CSV-Export: Teilnehmerliste mit Check-in-Status
- [ ] 🆕 CSV-Export: Bestellübersicht mit Zahlungsstatus
- [ ] 🆕 CSV-Export: Steuerbericht gruppiert nach Steuersatz
- [ ] Unit Tests für MetricsService Berechnungen
- [ ] Unit Tests für CSV-Export Formatierung
- [ ] E2E Test für Dashboard-Seite
