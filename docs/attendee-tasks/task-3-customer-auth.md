# Aufgabe 3: Customer Authentication & Account Management

## Übersicht

Implementiere die Kundenauthentifizierung mit Microsoft Entra External Identities. Kund\*innen sollen als Gast bestellen oder sich registrieren können, um ihre Bestellhistorie einzusehen.

## Kontext

- Admin-Authentifizierung (Entra ID mit App Registration) ist bereits konfiguriert
- Das Order-Model hat ein optionales `CustomerId` Feld
- MSAL.js ist bereits als Dependency installiert
- Entra External Identities Tenant wird vom Workshop-Leiter bereitgestellt

## Aufgaben

### Backend

1. **Customer Domain Model** (`Domain/Customers/`):
   - `Customer.cs`:
     - Id (von Entra External ID)
     - Email, Name
     - Typ: `Guest` oder `Registered`
     - Erstellungsdatum
     - Optional: Adresse, Telefon
   - `CustomerType.cs`: Enum mit `Guest`, `Registered`

2. **Customer Repository** (`Infrastructure/Cosmos/Repositories/CustomerRepository.cs`):
   - CRUD Operationen
   - Query: By Email, By ExternalId

3. **Identity Service** (`Infrastructure/Identity/EntraExternalIdService.cs`):
   - Token-Validierung für Entra External Identities
   - User-Info aus Token extrahieren
   - Customer-Profil aus Claims erstellen

4. **Auth Endpoints** (`Endpoints/AuthEndpoints.cs`):
   - `POST /api/v1/auth/guest-checkout` — Erstellt Gast-Customer mit Email
   - `GET /api/v1/customers/me` — Gibt eigenes Profil zurück (Auth erforderlich)
   - `PUT /api/v1/customers/me` — Profil aktualisieren
   - `GET /api/v1/customers/me/orders` — Eigene Bestellungen

5. **Auth Middleware erweitern**:
   - Dual-Auth Setup: Entra ID (Admin) + Entra External (Customer)
   - JWT Validation für beide Token-Typen
   - Claims-basierte Autorisierung

6. **Order-Erstellung anpassen**:
   - Wenn Customer eingeloggt → `CustomerId` setzen
   - Wenn Guest → nur Email erforderlich
   - Bestehenden Guest-Customer mit neuem Account verknüpfen

### Frontend

1. **Auth Provider konfigurieren** (`lib/auth.ts`):
   - Separater MSAL Provider für Customer-Auth (External Identities)
   - `useCustomerAuth()` Hook
   - Token-Management (Refresh, Expiry)

2. **Login-Seite** (`/account/login`):
   - "Mit Microsoft anmelden" Button (Entra External ID)
   - Email/Passwort Login (wenn in External ID konfiguriert)
   - Link zur Registrierung
   - Link zum Gast-Checkout

3. **Registrierungsseite** (`/account/register`):
   - Redirect zu Entra External ID Sign-up Flow
   - Nach Registrierung: Profil vervollständigen

4. **Mein Konto Seite** (`/account`):
   - Profilübersicht (Name, Email)
   - Profil bearbeiten
   - Bestellhistorie mit Status

5. **Bestellhistorie** (`/account/orders`):
   - Tabelle mit eigenen Bestellungen
   - Status (Bezahlt, Storniert, etc.)
   - Click → Order Detail mit Tickets und Rechnung

6. **Checkout-Flow anpassen**:
   - Option: "Als Gast bestellen" oder "Einloggen"
   - Wenn eingeloggt: Email und Name vorausgefüllt
   - Nach Kauf: "Konto erstellen?" Prompt für Gäste

### Hinweise

- Entra External Identities verwendet einen separaten Tenant
- Die MSAL Konfiguration unterscheidet sich für Admin und Customer
- Guest-zu-Account Migration: Alle Bestellungen mit gleicher Email verknüpfen
- PII (persönliche Daten) sicher speichern und DSGVO beachten

### Agents verwenden

- Nutze den **backend-api-agent** für Auth-Endpoints
- Nutze den **frontend-component-agent** für Login/Register/Account Seiten
- Nutze den **testing-agent** für Auth-Middleware Tests
- Nutze den **e2e-testing-agent** für Login-Flow Tests

### Akzeptanzkriterien

- [ ] Gast-Checkout funktioniert nur mit Email
- [ ] Registrierung über Entra External Identities
- [ ] Login und Token-Validierung funktionieren
- [ ] "Mein Konto" zeigt Profil und Bestellhistorie
- [ ] Bestehende Gast-Bestellungen werden nach Registrierung verknüpft
- [ ] Admin- und Customer-Auth koexistieren korrekt
- [ ] Unit Tests für Token-Validierung und Customer-Service
- [ ] E2E Test für Login und Bestellhistorie
