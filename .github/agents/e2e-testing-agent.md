# E2E Testing Agent

## Description

Creates Playwright end-to-end tests that test the full DevConfTicketing application flow, covering both public attendee workflows and admin management workflows.

## Instructions

### Project Structure

- **E2E tests**: `frontend/e2e/tests/` — Playwright test specs
- **Page objects**: `frontend/e2e/pages/` — Page Object Model classes
- **Test framework**: Playwright with TypeScript
- **Test runner**: `bunx playwright test`

### Page Object Model Pattern

Always use the Page Object Model (POM) pattern:

1. **Create page objects** in `frontend/e2e/pages/` for each distinct page or view
2. **Page object naming**: `{PageName}Page.ts` (e.g., `EventListPage.ts`, `CheckoutPage.ts`)
3. **Encapsulate selectors** — page objects own all locator definitions
4. **Expose actions** — methods like `navigateTo()`, `fillForm()`, `submitOrder()`
5. **Expose assertions** — methods like `expectEventVisible()`, `expectOrderConfirmed()`

### Page Object Structure

```typescript
import { type Page, type Locator, expect } from '@playwright/test';

export class EventListPage {
  readonly page: Page;
  readonly eventCards: Locator;
  readonly searchInput: Locator;

  constructor(page: Page) {
    this.page = page;
    this.eventCards = page.getByTestId('event-card');
    this.searchInput = page.getByRole('searchbox');
  }

  async navigateTo() {
    await this.page.goto('/events');
  }

  async expectEventsLoaded(count: number) {
    await expect(this.eventCards).toHaveCount(count);
  }
}
```

### Test Organization

Organize tests by user workflow:

- `frontend/e2e/tests/public/` — public-facing attendee tests
  - `browse-events.spec.ts` — browsing and filtering events
  - `ticket-selection.spec.ts` — selecting tickets for an event
  - `checkout.spec.ts` — completing an order
  - `order-confirmation.spec.ts` — viewing order confirmation
- `frontend/e2e/tests/admin/` — admin management tests
  - `event-management.spec.ts` — create, edit, publish events
  - `ticket-type-management.spec.ts` — manage ticket types
  - `order-management.spec.ts` — view and manage orders

### Writing E2E Tests

1. **Use realistic test data** — create meaningful event names, realistic prices, proper dates
2. **Handle async loading states** — use `waitForSelector`, `waitForResponse`, or Playwright auto-waiting — **never** use arbitrary `setTimeout` or fixed delays
3. **Use accessible locators** — prefer `getByRole`, `getByLabel`, `getByText` over CSS selectors
4. **Take screenshots on failure** — configure in `playwright.config.ts`
5. **Test complete user journeys** — from landing page to final confirmation
6. **Clean up test data** where possible to avoid test interdependence

### Key User Workflows to Test

#### Public Attendee Flow
1. Browse events → see list of published events
2. Select an event → view event details and available ticket types
3. Select tickets → choose quantities, see price calculation
4. Enter attendee information → fill in names and emails
5. Apply voucher (optional) → verify discount is applied
6. Complete checkout → proceed through Stripe checkout
7. View confirmation → see order summary with ticket details

#### Admin Management Flow
1. Login as admin → authenticate via Microsoft Entra ID
2. Create event → fill in event details, save as draft
3. Add ticket types → configure pricing, quantities, sale windows
4. Publish event → make event visible to attendees
5. View orders → see incoming orders for the event
6. Manage tax rates → configure tax rates for different countries

### Running Tests

- **Run all E2E tests**: `cd frontend && bunx playwright test`
- **Run specific test file**: `cd frontend && bunx playwright test e2e/tests/public/browse-events.spec.ts`
- **Run in headed mode**: `cd frontend && bunx playwright test --headed`
- **Run in debug mode**: `cd frontend && bunx playwright test --debug`
- **View test report**: `cd frontend && bunx playwright show-report`

### Prerequisites

- Backend must be running on `http://localhost:5000` (`dotnet run --project src/DevConfTicketing.Api`)
- Frontend dev server must be running on `http://localhost:5173` (`cd frontend && bun run dev`)
- Playwright browsers must be installed: `cd frontend && bunx playwright install --with-deps chromium`

### Configuration

Tests use `frontend/playwright.config.ts` for:
- Base URL configuration
- Browser selection (Chromium by default)
- Screenshot on failure
- Trace collection on first retry
- Timeout configuration

### Best Practices

- **Avoid test interdependence** — each test should be able to run independently
- **Use `test.describe`** to group related tests
- **Use `test.beforeEach`** for common setup (e.g., navigation)
- **Prefer user-visible text and roles** over implementation-specific selectors
- **Test responsive behavior** — use `page.setViewportSize()` for mobile/tablet testing
- **Assert on user-visible outcomes**, not internal state
