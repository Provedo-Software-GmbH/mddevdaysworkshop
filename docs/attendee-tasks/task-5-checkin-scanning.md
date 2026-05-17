# Aufgabe 5: Check-in & Ticket Scanning

> ⚠️ **Ersetzt die ursprüngliche Aufgabe "Event Sessions & Speakers"**
> Inspiriert von pretix: Check-in ist für ein produktives Ticketing-System essentiell. pretix hat Check-in-Listen, eine Web-Scanner-App, Such-API und Statistiken. Sessions/Speaker können später ergänzt werden.

## Übersicht

Implementiere ein vollständiges Check-in-System für Events. Event-Personal soll Tickets per QR-Code scannen können (mobil über die Browser-Kamera), Teilnehmer\*innen suchen und Check-in-Statistiken einsehen können.

## Kontext

- Orders haben bereits `Positions[]` mit `TicketSecret` und `CheckedInAt` Feldern (vorgebaut)
- QR-Codes werden von Aufgabe 4 generiert und enthalten das `TicketSecret`
- Die Admin-Oberfläche ist vorgebaut mit Sidebar-Navigation
- Es gibt keine Check-in-Funktionalität
- Frontend OpenTelemetry ist vorkonfiguriert (`lib/telemetry.ts`): `fetch()` Calls senden automatisch `traceparent` Header für End-to-End Tracing. Nutze `startSpan()` für Scanner- und Check-in-Aktionen

## Aufgaben

### Backend

1. **CheckIn Domain Model** (`Domain/CheckIn/`):
   - `CheckInList.cs`:
     - Id, EventId
     - Name (z.B. "Haupteingang", "Workshop-Raum A", "Abendessen")
     - Filter: `AllTicketTypes` (bool) oder `TicketTypeIds[]` (nur bestimmte Ticket-Typen)
     - `IncludePending` (bool) — auch unbezahlte Bestellungen erlauben
     - `AllowMultipleEntries` (bool) — Doppel-Scan erlauben
     - Status-Zähler: `CheckedInCount`, `TotalCount`
   - `CheckInRecord.cs`:
     - Id, EventId, CheckInListId
     - OrderId, PositionIndex
     - TicketSecret (das gescannte Secret)
     - `ScannedAt` (Zeitpunkt)
     - `ScannedBy` (Admin-User ID)
     - `Type`: `Entry` | `Exit` (für Stretch Goal)
     - `Result`: `Success` | `AlreadyCheckedIn` | `InvalidTicket` | `Unpaid` | `Cancelled` | `WrongList`

2. **CheckIn Repository** (`Infrastructure/Cosmos/Repositories/CheckInRepository.cs`):
   - CRUD für CheckInList
   - CheckIn-Record erstellen
   - Query: Records by EventId, by ListId
   - Query: Statistiken (checked-in count, total)

3. **CheckIn Service** (`Application/CheckIn/CheckInService.cs`):
   - `RedeemTicket(listId, ticketSecret)` — Hauptfunktion:
     1. `TicketSecret` in `OrderPositions` suchen
     2. Validieren: Order-Status ist `Paid` (oder `Pending` wenn `IncludePending`)
     3. Validieren: Ticket-Typ passt zur CheckInList
     4. Validieren: Noch nicht eingecheckt (oder `AllowMultipleEntries`)
     5. CheckIn-Record erstellen
     6. `CheckedInAt` auf der OrderPosition setzen
     7. Result zurückgeben mit Teilnehmer-Infos
   - `SearchAttendee(eventId, query)` — Suche nach Name oder Bestellcode
   - `AnnulCheckIn(checkInRecordId)` — Check-in rückgängig machen
   - `GetCheckInStats(eventId, listId)` — Statistiken

4. **CheckIn Endpoints** (`Endpoints/CheckInEndpoints.cs`):
   - **Check-in Listen (Admin)**:
     - `GET /api/v1/events/{eventId}/checkin-lists` — Alle Listen
     - `POST /api/v1/events/{eventId}/checkin-lists` — Liste erstellen
     - `PUT /api/v1/events/{eventId}/checkin-lists/{id}` — Liste bearbeiten
     - `DELETE /api/v1/events/{eventId}/checkin-lists/{id}` — Liste löschen
   - **Scanning (Staff)**:
     - `POST /api/v1/events/{eventId}/checkin/redeem` — QR-Code scannen → Ticket einlösen
       - Request: `{ "listId": "...", "secret": "..." }`
       - Response: `{ "result": "Success", "attendee": { "name": "...", "ticketType": "...", "orderCode": "..." } }`
     - `GET /api/v1/events/{eventId}/checkin/search?q=Max` — Teilnehmer suchen
     - `POST /api/v1/events/{eventId}/checkin/{recordId}/annul` — Check-in rückgängig
   - **Statistiken**:
     - `GET /api/v1/events/{eventId}/checkin/stats` — Gesamtstatistik
     - `GET /api/v1/events/{eventId}/checkin-lists/{id}/stats` — Pro Liste
   - **Export**:
     - `GET /api/v1/events/{eventId}/checkin/export?format=csv` — CSV-Export der Teilnehmerliste mit Check-in-Status

### Frontend

1. **Check-in Listen verwalten (Admin)** (`/admin/events/:id/checkin`):
   - Tabelle aller Check-in-Listen mit Name und Statistik (z.B. "45/120 eingecheckt")
   - Erstellen/Bearbeiten Form:
     - Name
     - Ticket-Typ Filter (Multi-Select oder "Alle")
     - Optionen: Unbezahlte erlauben, Mehrfach-Scan erlauben
   - Progress-Bar für jede Liste (checked-in / total)
   - CSV-Export Button

2. **QR-Scanner Seite (Staff)** (`/admin/events/:id/scan`):
   - **Dies ist die wichtigste Seite — Mobile-First Design!**
   - Dropdown: Check-in-Liste auswählen
   - Kamera-Ansicht für QR-Code Scan (z.B. `html5-qrcode` oder `@yudiel/react-qr-scanner`)
   - Nach Scan:
     - ✅ Erfolg: Grüner Bildschirm mit Teilnehmername, Ticket-Typ, Bestellcode
     - ⚠️ Bereits eingecheckt: Gelber Bildschirm mit Warnung und Zeitpunkt des ersten Check-ins
     - ❌ Ungültig: Roter Bildschirm mit Fehlermeldung
   - Sound-Feedback (Erfolg/Fehler)
   - Manuelle Eingabe als Fallback (Bestellcode oder Name tippen)
   - Letzte Scans als Liste unter dem Scanner

3. **Teilnehmer-Suche** (Teil der Scan-Seite):
   - Suchfeld: Name oder Bestellcode
   - Ergebnisliste mit Check-in-Status
   - "Manuell einchecken" Button pro Teilnehmer

4. **Check-in Statistik (Admin)** (`/admin/events/:id/checkin/stats`):
   - KPI-Cards: Eingecheckt / Gesamt, Prozent
   - Progress-Bar pro Check-in-Liste
   - Live-Updates (Polling alle 10 Sekunden)
   - Zeitleiste: Check-ins über Zeit (wann kamen die meisten Leute?)

### Hinweise

- **QR-Scanner Library**: `html5-qrcode` oder `@yudiel/react-qr-scanner` für Browser-basiertes Scannen
- **Mobile-First**: Die Scan-Seite wird primär auf Smartphones genutzt
- **Sound**: Verwende Web Audio API für akustisches Feedback
- **Offline**: Nicht nötig — wir gehen von stabiler Internetverbindung aus
- **Performance**: Check-in Validierung muss schnell sein (< 200ms)
- Das `TicketSecret` ist ein kryptografisch sicherer String (vorgebaut), NICHT erratbar
- Check-in Records sind immutable (Append-only) — nur Annulierung möglich

### Agents verwenden

- Nutze den **backend-api-agent** für CheckIn Endpoints und Service
- Nutze den **frontend-component-agent** für die Scanner-UI
- Nutze den **testing-agent** für:
  - CheckInService Tests (alle Validierungs-Szenarien)
  - Doppel-Scan Tests
  - Falsches Ticket Tests
  - Statistik-Berechnung Tests
- Nutze den **e2e-testing-agent** für:
  - Check-in-Liste erstellen
  - Manuellen Check-in durchführen
  - Statistik-Ansicht prüfen
  - (QR-Scan ist schwer E2E zu testen — manuellen Check-in verwenden)

### Akzeptanzkriterien

- [ ] Check-in-Listen können erstellt und verwaltet werden
- [ ] QR-Code Scanner funktioniert im Browser (Smartphone-Kamera)
- [ ] Erfolgreicher Scan zeigt Teilnehmerdaten (Name, Ticket-Typ, Bestellcode)
- [ ] Doppel-Scan wird erkannt und gewarnt
- [ ] Ungültige/stornierte Tickets werden abgelehnt
- [ ] Teilnehmer können per Name/Bestellcode gesucht werden
- [ ] Manueller Check-in ohne QR-Code möglich
- [ ] Check-in Statistiken (eingecheckt / gesamt) werden angezeigt
- [ ] CSV-Export der Teilnehmerliste mit Check-in-Status
- [ ] Mobile-responsive Scanner-Seite
- [ ] Sound-Feedback bei Scan (Erfolg/Fehler)
- [ ] Unit Tests für CheckInService (alle Validierungs-Szenarien)
- [ ] E2E Test für Check-in-Flow (manuell)
- [ ] Frontend-Telemetry: Custom Spans für Scanner-Aktionen (z.B. `checkin.scan`, `checkin.manual`, `checkin.search`) via `lib/telemetry.ts`
