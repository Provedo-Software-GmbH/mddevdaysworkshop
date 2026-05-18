# Workshop Guide — KI-gestützte Entwicklung mit GitHub Copilot Agents

## Willkommen! 🎉

In diesem Workshop lernst du, wie du mit GitHub Copilot Agents produktiv entwickelst. Du arbeitest an einem echten Feature für unser Event-Ticketing-System — nicht an einer Übungsaufgabe, sondern an Code, der produktiv eingesetzt wird.

## Zeitplan

| Zeit | Block |
| ---- | ----- |
| 09:00 – 10:00 | Intro: Agentenbasierte Entwicklung, Mindset, Tooling |
| 10:00 – 11:00 | Demo: Repository-Walkthrough, Agent Setup, Model-Auswahl, erstes Issue |
| 11:00 – 11:15 | Pause |
| 11:15 – 13:15 | Hands-on Block 1: Jede\*r arbeitet an eigenem Feature |
| 13:15 – 14:00 | Mittagspause |
| 14:00 – 16:00 | Hands-on Block 2: Feature fertigstellen, PRs reviewen |
| 16:00 – 16:30 | Wrap-up: Erfahrungen, Lessons Learned, Q&A |

## Vorbereitung (vor dem Workshop)

### 1. Zugang prüfen

- [ ] GitHub Account mit **GitHub Copilot** Zugang (Business oder Enterprise)
- [ ] Copilot Coding Agent aktiviert (in den Copilot-Einstellungen)
- [ ] Zugang zum Repository `sia-consulting/mddevdaysworkshop`

### 2. Lokale Entwicklungsumgebung (optional, aber empfohlen)

Falls du lokal testen möchtest:

```bash
# .NET 11 Preview SDK installieren
# https://dotnet.microsoft.com/download/dotnet/11.0

# Bun installieren
curl -fsSL https://bun.sh/install | bash

# Repository klonen
git clone https://github.com/sia-consulting/mddevdaysworkshop.git
cd mddevdaysworkshop

# Backend starten
dotnet run --project src/DevConfTicketing.Api

# Frontend starten (neues Terminal)
cd frontend && bun install && bun dev
```

### 3. Repository kennenlernen

Schau dir vorab diese Dateien an:
- `README.md` — Setup-Anleitung
- `docs/00-workshop-overview.md` — Was wir bauen
- `docs/01-architecture-plan.md` — Architektur-Übersicht
- Dein zugewiesenes Task-Dokument in `docs/attendee-tasks/`

## Dein Workflow am Workshop-Tag

### Schritt 1: Fork erstellen

1. Gehe zu `github.com/sia-consulting/mddevdaysworkshop`
2. Klicke auf **Fork** → erstelle einen Fork in deiner Organisation
3. Stelle sicher, dass Copilot Agent Zugang zum Fork hat

### Schritt 2: Dein Issue finden

Du bekommst ein GitHub Issue zugewiesen (z.B. "Stripe Payment Integration"). Das Issue enthält:
- Eine Beschreibung des Features
- Akzeptanzkriterien als Checkliste
- Hinweise auf relevanten bestehenden Code

### Schritt 3: Copilot Agent starten

1. Öffne dein Issue
2. Weise das Issue dem **Copilot Coding Agent** zu
3. Der Agent:
   - Liest das Issue und die Akzeptanzkriterien
   - Analysiert den bestehenden Code
   - Erstellt einen Plan
   - Implementiert das Feature
   - Erstellt einen Pull Request

### Schritt 4: PR reviewen & iterieren

1. Schau dir den erstellten PR an
2. Review den Code — prüfe:
   - Werden die Akzeptanzkriterien erfüllt?
   - Folgt der Code den Projekt-Konventionen?
   - Gibt es offensichtliche Bugs oder Sicherheitslücken?
3. Hinterlasse Kommentare auf dem PR:
   - "Die Fehlerbehandlung für ungültige Voucher fehlt"
   - "Bitte füge Tests für den Edge-Case hinzu"
   - "Der Endpunkt sollte auch die Order-ID zurückgeben"
4. Der Agent nimmt die Änderungen vor
5. Wiederhole bis zufrieden

### Schritt 5: Fertigstellen

1. Erstelle einen PR gegen das Hauptrepo
2. Präsentiere dein Feature in der Wrap-up Session

## Die 6 Workshop-Features

| # | Feature | Attendee |
| - | ------- | -------- |
| 1 | Stripe Payment, Refunds & Cancellations | — |
| 2 | Invoice, Tax Calculation & Cancellation Invoices | — |
| 3 | Customer Auth & Account Management | — |
| 4 | Ticket PDF, QR-Codes & Email Notifications | — |
| 5 | Check-in & Ticket Scanning | — |
| 6 | Admin Dashboard, KPIs & Data Export | — |

Detaillierte Beschreibungen: `docs/attendee-tasks/`

## Tipps für die Arbeit mit dem Copilot Agent

### Do's ✅

- **Klare Issues schreiben**: Je präziser die Akzeptanzkriterien, desto besser das Ergebnis
- **Iterativ arbeiten**: Lieber mehrere kleine Durchgänge als alles auf einmal
- **Bestehenden Code referenzieren**: "Implementiere es analog zu EventEndpoints.cs"
- **Feedback geben**: Kommentare auf dem PR helfen dem Agent, besser zu werden
- **Tests verlangen**: "Füge Unit Tests analog zu den bestehenden Tests hinzu"

### Don'ts ❌

- **Nicht blind mergen**: Immer den generierten Code reviewen
- **Nicht zu viel auf einmal**: Ein Issue = ein Feature = ein PR
- **Nicht die Architektur ändern**: Die Grundstruktur steht — baue darauf auf
- **Nicht auf perfekte erste Iteration warten**: Iteriere über PR-Kommentare

### Modell-Auswahl

Der Copilot Agent kann verschiedene Modelle nutzen. Für diesen Workshop:
- **Claude Sonnet 4** — Gute Balance aus Qualität und Geschwindigkeit (Default)
- **Claude Opus 4** — Für komplexe Architektur-Entscheidungen
- **GPT-4.1** — Alternative bei spezifischen Anforderungen

## Hilfreiche Kommandos

```bash
# Backend
dotnet build src/DevConfTicketing.slnx           # Build
dotnet test src/DevConfTicketing.slnx            # Tests
dotnet watch --project src/DevConfTicketing.Api  # Hot Reload

# Frontend
cd frontend
bun install          # Dependencies installieren
bun dev              # Dev Server starten
bun test             # Unit Tests
bun lint             # Linting
bun run build        # Production Build
bunx playwright test # E2E Tests
```

## Architektur auf einen Blick

```
Frontend (React 19)          Backend (.NET 11 Minimal APIs)
├── Public Pages             ├── Endpoints/
│   ├── Event Listing        │   ├── EventEndpoints.cs
│   ├── Event Detail         │   ├── TicketTypeEndpoints.cs
│   └── Ticket Selection     │   ├── OrderEndpoints.cs
├── Admin Pages              │   └── ...dein Feature...
│   ├── Event Management     ├── Application/
│   ├── Ticket Types         │   ├── Handlers (Create, Update, Get)
│   └── Tax Rates            │   └── Services (TaxCalculation)
└── Shared Components        ├── Domain/
    ├── DataTable            │   └── Models (Event, Order, TicketType)
    ├── Forms                └── Infrastructure/
    └── UI (shadcn/ui)          ├── CosmosDbService
                                ├── Repositories
                                └── TelemetryService
```

## Fragen?

- **Während des Workshops**: Frag den Workshop-Leiter
- **Technische Probleme**: Erstelle ein Issue mit dem Label `help-wanted`
- **Code-Fragen**: Nutze Copilot Chat in deiner IDE

Viel Erfolg und Spaß beim Workshop! 🚀
