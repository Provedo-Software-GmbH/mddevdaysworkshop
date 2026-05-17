# Aufgabe 2: Invoice, Tax Calculation & Cancellation Invoices

## Übersicht

Implementiere eine vollständige Rechnungserstellung mit korrekter deutscher Steuerberechnung. Rechnungen sollen nach Bezahlung automatisch generiert werden können und alle gesetzlichen Anforderungen erfüllen. Bei Stornierung wird automatisch eine Stornorechnung erstellt.

> **Inspiriert von pretix**: pretix generiert automatisch Stornorechnungen bei Bestellungsstornierung — ein gesetzliches Muss in Deutschland.

## Kontext

- Tax Rates sind bereits im System angelegt (19%, 7%, 0% für Beherbergungssteuer)
- Das Order-Model enthält bereits `LineItems` mit Steuerinformationen
- Eine grundlegende `TaxCalculationService` existiert bereits
- Der **german-tax-calculation** Skill enthält alle relevanten Steuerregeln

## Aufgaben

### Backend

1. **Invoice Domain Model** (`Domain/Invoices/`):
   - `Invoice.cs` — Rechnung mit:
     - Fortlaufende Rechnungsnummer (z.B. `INV-2026-00001`)
     - Rechnungsdatum
     - Referenz zur Order
     - Kundendaten (Name, Email, optional Adresse)
     - Liste von `InvoiceLineItem`
     - Nettobetrag, Steuerbetrag, Bruttobetrag (jeweils pro Steuersatz)
     - Steuer-ID des Ausstellers
   - `InvoiceLineItem.cs` — Position mit Netto, Steuersatz, Steuer, Brutto

2. **Invoice Repository** (`Infrastructure/Cosmos/Repositories/InvoiceRepository.cs`):
   - CRUD Operationen
   - Fortlaufende Rechnungsnummer generieren (Counter-Dokument in Cosmos)
   - Query: Rechnungen nach Order, nach Zeitraum

3. **Tax Calculation Service erweitern** (`Application/Tickets/TaxCalculationService.cs`):
   - Steuerberechnung pro Steuersatz gruppieren
   - Rundungsregeln korrekt implementieren (kaufmännische Rundung auf 2 Dezimalstellen)
   - Steuer-Zusammenfassung (Tax Summary) erstellen:
     ```
     Nettobetrag 19%: 470,59 €  | Steuer: 89,41 €
     Nettobetrag  7%: 186,92 €  | Steuer: 13,08 €
     Nettobetrag  0%:  10,00 €  | Steuer:  0,00 €
     ─────────────────────────────────────────────
     Gesamt Netto:    667,51 €  | Gesamt Steuer: 102,49 €
     Gesamtbetrag:    770,00 €
     ```

4. **Invoice Generation Service** (`Application/Invoices/InvoiceGenerationService.cs`):
   - Invoice aus Order erstellen
   - PDF-Generierung (z.B. mit QuestPDF oder ähnlich)
   - Alle Pflichtangaben nach §14 UStG

5. **Invoice Endpoints** (`Endpoints/InvoiceEndpoints.cs`):
   - `POST /api/v1/orders/{id}/invoice/generate` — Rechnung erstellen
   - `GET /api/v1/orders/{id}/invoice` — Rechnungsdaten abrufen
   - `GET /api/v1/orders/{id}/invoice/pdf` — PDF herunterladen

6. **Stornorechnung (Cancellation Invoice)** (🆕 inspiriert von pretix):
   - Wird automatisch erstellt wenn eine Bestellung storniert wird (Integration mit Attendee 1)
   - Eigene fortlaufende Nummer (z.B. `STORNO-2026-00001`) oder gleiche Nummernkreis
   - Referenziert die Original-Rechnung
   - Enthält die gleichen Positionen mit negativen Beträgen
   - Invoice-Typ: `Invoice` vs `CancellationInvoice`
   - `POST /api/v1/orders/{id}/invoice/cancel` — Stornorechnung manuell erstellen

### Frontend

1. **Invoice-Ansicht im Admin** (`/admin/invoices`):
   - Tabelle aller Rechnungen mit Nummer, Datum, Betrag, Status
   - Filter nach Event, Zeitraum
   - PDF-Download Button

2. **Invoice-Detail im Order-Detail**:
   - Tax Breakdown Komponente erweitern
   - Steuer-Zusammenfassung anzeigen
   - "Rechnung generieren" Button (wenn noch keine existiert)
   - "PDF herunterladen" Button

3. **Steueraufschlüsselung in der öffentlichen Ticket-Ansicht**:
   - Bei Ticket-Auswahl die Positionen mit Steuersätzen anzeigen
   - Gesamtpreis mit Hinweis "inkl. MwSt." und detaillierter Aufschlüsselung

### Hinweise

- Rundung immer kaufmännisch auf 2 Dezimalstellen
- Rechnungsnummern müssen fortlaufend und lückenlos sein
- PDF muss alle Pflichtangaben enthalten (§14 UStG)
- Beherbergungssteuer ist keine Umsatzsteuer — separat ausweisen

### Agents verwenden

- Nutze den **backend-api-agent** für Endpoints und Services
- Nutze den **frontend-component-agent** für die Invoice-UI
- Nutze den **testing-agent** für Tax Calculation Tests (verschiedene Szenarien)
- Der **german-tax-calculation** Skill ist essentiell für diese Aufgabe

### Akzeptanzkriterien

- [ ] Rechnungen werden korrekt aus Orders erstellt
- [ ] Fortlaufende Rechnungsnummern werden korrekt generiert
- [ ] Steuerberechnung ist korrekt für alle Steuersätze (19%, 7%, 0%)
- [ ] Steuer-Zusammenfassung gruppiert nach Steuersatz
- [ ] PDF enthält alle Pflichtangaben nach §14 UStG
- [ ] Admin kann Rechnungen einsehen und PDFs herunterladen
- [ ] 🆕 Stornorechnung wird bei Stornierung automatisch erstellt
- [ ] 🆕 Stornorechnung referenziert die Originalrechnung
- [ ] Unit Tests für TaxCalculationService mit verschiedenen Szenarien
- [ ] Unit Tests für Invoice-Nummern-Generierung
- [ ] 🆕 Unit Tests für Stornorechnung-Erstellung
