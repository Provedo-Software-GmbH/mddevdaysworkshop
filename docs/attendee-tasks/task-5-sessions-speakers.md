# Aufgabe 5: Event Session & Speaker Management

## Übersicht

Implementiere die Verwaltung von Vorträgen (Sessions) und Referent\*innen (Speakers) pro Event. Besucher\*innen sollen das Programm einsehen können, Admins sollen Sessions und Speaker verwalten.

## Kontext

- Events existieren bereits mit CRUD API und Frontend
- Es gibt keine Session- oder Speaker-Funktionalität
- Die öffentliche Event-Detail-Seite existiert und kann erweitert werden

## Aufgaben

### Backend

1. **Session Domain Model** (`Domain/Sessions/`):
   - `Session.cs`:
     - Id, EventId
     - Titel, Beschreibung (Abstract)
     - Startzeit, Endzeit
     - Raum / Track (z.B. "Hauptbühne", "Workshop-Raum A")
     - Schwierigkeitsgrad: `Beginner`, `Intermediate`, `Advanced`
     - Typ: `Talk`, `Workshop`, `Keynote`, `Panel`, `Break`
     - Liste von SpeakerIds
     - Tags (z.B. "AI", "Cloud", "Frontend")
     - Status: `Draft`, `Confirmed`, `Cancelled`
   - `Speaker.cs`:
     - Id, EventId
     - Name, Bio
     - Firma, Jobtitel
     - Foto-URL
     - Social Links (Twitter, LinkedIn, GitHub)
     - Liste von SessionIds

2. **Repositories** (`Infrastructure/Cosmos/Repositories/`):
   - `SessionRepository.cs` — CRUD, Query by EventId, by Track, by Speaker
   - `SpeakerRepository.cs` — CRUD, Query by EventId

3. **Session Service** (`Application/Sessions/SessionService.cs`):
   - Session erstellen mit Speaker-Zuordnung
   - Zeitkonflikt-Prüfung (kein Speaker in zwei Sessions gleichzeitig)
   - Programm-Übersicht generieren (Sessions nach Tag und Zeit sortiert)
   - Filtern nach Track, Tag, Speaker, Schwierigkeitsgrad

4. **Session Endpoints** (`Endpoints/SessionEndpoints.cs`):
   - `GET /api/v1/events/{eventId}/sessions` — Alle Sessions (mit Filtern)
   - `GET /api/v1/events/{eventId}/sessions/{id}` — Session Detail
   - `POST /api/v1/events/{eventId}/sessions` — Session erstellen
   - `PUT /api/v1/events/{eventId}/sessions/{id}` — Session aktualisieren
   - `DELETE /api/v1/events/{eventId}/sessions/{id}` — Session löschen
   - `GET /api/v1/events/{eventId}/schedule` — Programm-Übersicht (nach Tag/Zeit)

5. **Speaker Endpoints** (`Endpoints/SpeakerEndpoints.cs`):
   - `GET /api/v1/events/{eventId}/speakers` — Alle Speaker
   - `GET /api/v1/events/{eventId}/speakers/{id}` — Speaker Detail mit Sessions
   - `POST /api/v1/events/{eventId}/speakers` — Speaker erstellen
   - `PUT /api/v1/events/{eventId}/speakers/{id}` — Speaker aktualisieren
   - `DELETE /api/v1/events/{eventId}/speakers/{id}` — Speaker löschen

### Frontend

1. **Programm-Seite (Public)** (`/events/:id/sessions`):
   - Tagesübersicht mit Zeitleiste
   - Sessions als Karten in Tracks/Spalten (Grid-Layout)
   - Farb-Codierung nach Track oder Session-Typ
   - Filter: Tag, Track, Schwierigkeitsgrad, Tags
   - Click auf Session → Session-Detail Modal/Sheet
   - Click auf Speaker → Speaker-Profil

2. **Speaker-Übersicht (Public)** (Teil der Event-Seite):
   - Grid mit Speaker-Karten (Foto, Name, Firma)
   - Click → Speaker Detail mit Bio und Sessions

3. **Session-Verwaltung (Admin)** (`/admin/events/:id/sessions`):
   - Tabelle aller Sessions mit Status
   - Session erstellen/bearbeiten Form:
     - Titel, Beschreibung
     - Start/Endzeit (DateTime Picker)
     - Raum/Track Auswahl
     - Speaker-Zuordnung (Multi-Select)
     - Tags (Tag Input)
     - Typ und Schwierigkeitsgrad
   - Drag & Drop für Zeitplan-Erstellung (optional, Stretch Goal)

4. **Speaker-Verwaltung (Admin)** (`/admin/events/:id/speakers`):
   - Tabelle aller Speaker
   - Speaker erstellen/bearbeiten Form:
     - Name, Bio, Firma, Jobtitel
     - Foto-URL
     - Social Links
   - Session-Zuordnung anzeigen

5. **Event-Detail Seite erweitern**:
   - Tab oder Abschnitt "Programm" mit Link zur Session-Seite
   - Speaker-Vorschau auf der Event-Seite

### Hinweise

- Sessions und Speaker sind immer einem Event zugeordnet
- Ein Speaker kann mehrere Sessions haben
- Eine Session kann mehrere Speaker haben (z.B. Panel)
- Zeitkonflikte pro Speaker prüfen
- Das Programm sollte auch ohne Sessions funktionieren (leerer Zustand)

### Agents verwenden

- Nutze den **backend-api-agent** für Session/Speaker Endpoints
- Nutze den **frontend-component-agent** für die Programm-UI
- Nutze den **testing-agent** für:
  - Zeitkonflikt-Prüfung Tests
  - CRUD Tests für Sessions und Speaker
  - Schedule-Generierung Tests
- Nutze den **e2e-testing-agent** für:
  - Session erstellen und im Programm anzeigen
  - Speaker-Profil aufrufen

### Akzeptanzkriterien

- [ ] Sessions und Speaker können erstellt, bearbeitet, gelöscht werden
- [ ] Sessions sind einem Event und Speakern zugeordnet
- [ ] Programm-Seite zeigt Sessions als Zeitleiste/Grid
- [ ] Filter funktionieren (Track, Tag, Schwierigkeitsgrad)
- [ ] Speaker-Seite zeigt Profil und zugehörige Sessions
- [ ] Zeitkonflikt-Prüfung für Speaker
- [ ] Admin kann Sessions und Speaker verwalten
- [ ] Responsive Design für Programm-Seite (Mobile tauglich)
- [ ] Unit Tests für SessionService (Konflikte, Sortierung)
- [ ] E2E Test für Session-Erstellung und Programm-Ansicht
