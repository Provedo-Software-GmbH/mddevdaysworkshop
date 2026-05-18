# Accessibility Checker

## Description

Reviews UI components for WCAG 2.1 AA compliance, covering color contrast, keyboard navigation, ARIA attributes, focus management, and screen reader compatibility for the DevConfTicketing frontend application.

## Color Contrast Requirements

### WCAG 2.1 AA Minimum Ratios

| Element | Minimum Contrast Ratio |
|---------|----------------------|
| Normal text (< 18pt / < 14pt bold) | 4.5:1 |
| Large text (≥ 18pt / ≥ 14pt bold) | 3:1 |
| UI components and graphical objects | 3:1 |
| Focus indicators | 3:1 against adjacent colors |

### Checking Contrast in This Project

The project uses Tailwind CSS with CSS variables defined in `frontend/src/index.css`. Check that:
- Text colors (`text-foreground`, `text-muted-foreground`) meet 4.5:1 against their backgrounds
- Interactive elements (`primary`, `secondary`, `destructive`) meet 3:1
- Disabled states still meet 3:1 for non-text elements (buttons, inputs)
- Error messages (`text-destructive`) meet 4.5:1 against the form background

## Keyboard Navigation

### Requirements

- **All interactive elements** must be reachable via Tab key
- **Tab order** must follow a logical reading order (left-to-right, top-to-bottom)
- **Focus must be visible** — never use `outline-none` without a visible alternative
- **Escape key** should close modals, dropdowns, and dialogs
- **Enter/Space** should activate buttons and links
- **Arrow keys** should navigate within composite widgets (tabs, menus, listboxes)

### Common Patterns

#### Modals/Dialogs
- Focus moves to the dialog when it opens
- Focus is trapped inside the dialog (Tab cycles within)
- Escape closes the dialog
- Focus returns to the trigger element when closed
- shadcn/ui `Dialog` component handles this automatically

#### Dropdown Menus
- Enter/Space opens the menu
- Arrow keys navigate menu items
- Enter selects the focused item
- Escape closes the menu
- shadcn/ui `DropdownMenu` handles this automatically

#### Forms
- Tab moves between form fields
- Labels are associated with inputs (`htmlFor` / `id` pairing or wrapping `<label>`)
- Required fields are indicated (not just by color)
- Error messages are announced to screen readers
- Submit button is reachable via Tab after the last field

## ARIA Roles and Attributes

### When to Use ARIA

1. **Prefer semantic HTML first** — `<button>`, `<nav>`, `<main>`, `<section>`, `<article>`
2. **Add ARIA only when semantic HTML is insufficient**
3. **Never use ARIA to override semantic meaning** of an element

### Common ARIA Patterns for This Project

#### Event Cards
```tsx
<article aria-labelledby={`event-title-${event.id}`}>
  <h3 id={`event-title-${event.id}`}>{event.title}</h3>
  <p>{event.description}</p>
  <span aria-label={`Price: ${event.price} euros`}>{event.price}€</span>
</article>
```

#### Ticket Quantity Selector
```tsx
<div role="spinbutton" aria-label="Number of tickets" aria-valuenow={quantity} aria-valuemin={0} aria-valuemax={maxPerOrder}>
  <button aria-label="Decrease quantity" onClick={decrement}>-</button>
  <span>{quantity}</span>
  <button aria-label="Increase quantity" onClick={increment}>+</button>
</div>
```

#### Loading States
```tsx
<div aria-live="polite" aria-busy={isLoading}>
  {isLoading ? <Spinner aria-label="Loading events" /> : <EventList events={events} />}
</div>
```

#### Status Badges
```tsx
<Badge aria-label={`Event status: ${status}`}>{status}</Badge>
```

#### Order Summary
```tsx
<table aria-label="Order summary">
  <thead>
    <tr>
      <th scope="col">Ticket</th>
      <th scope="col">Quantity</th>
      <th scope="col">Price</th>
    </tr>
  </thead>
  <tbody>
    {items.map(item => (
      <tr key={item.id}>
        <td>{item.name}</td>
        <td>{item.quantity}</td>
        <td aria-label={`${item.price} euros`}>{item.price}€</td>
      </tr>
    ))}
  </tbody>
</table>
```

## Focus Management in SPAs

### Route Changes
- When navigating between pages, move focus to the main content area or page heading
- Announce the new page to screen readers using `aria-live` or document title changes
- The project uses React Router — hook into route changes to manage focus

```tsx
// In page components, focus the main heading on mount
useEffect(() => {
  const heading = document.querySelector('h1');
  heading?.focus();
}, []);
```

### Dynamic Content
- When new content loads (e.g., after a filter, search, or pagination):
  - Announce the change via `aria-live="polite"` region
  - Move focus to the results area or first result
  - Update the document title if the content change is significant

## Screen Reader Compatibility

### Best Practices

- **Alt text for images**: Every `<img>` must have an `alt` attribute
  - Decorative images: `alt=""`
  - Informative images: descriptive text
  - Event images: `alt={event.title}` or more descriptive text

- **Headings hierarchy**: Use `h1` → `h2` → `h3` without skipping levels
  - Each page should have exactly one `h1`
  - Section headings should be `h2`
  - Sub-sections should be `h3`

- **Link text**: Links should have descriptive text, not "click here"
  - ❌ `<a href="...">Click here</a>`
  - ✅ `<a href="...">View event details for {event.title}</a>`

- **Form validation errors**: Connect error messages to form fields
  ```tsx
  <input id="email" aria-describedby="email-error" aria-invalid={!!errors.email} />
  {errors.email && <p id="email-error" role="alert">{errors.email.message}</p>}
  ```

## shadcn/ui Accessibility Features

shadcn/ui components are built on Radix UI primitives which have excellent accessibility. However, verify:

- **Dialog**: Focus trap and Escape handling work correctly
- **Select**: Keyboard navigation with arrow keys
- **Toast**: Announcements via `aria-live` regions
- **Tabs**: Arrow key navigation between tabs, proper `role="tablist"` / `role="tab"` / `role="tabpanel"`
- **Badge**: Add `aria-label` for context (e.g., status badges)
- **Button**: Ensure disabled buttons have `aria-disabled="true"` and not just visual styling

## Checklist for Reviewing Components

When reviewing a component for accessibility:

- [ ] All interactive elements are keyboard accessible
- [ ] Focus indicators are visible (not removed with `outline-none`)
- [ ] Color contrast meets WCAG AA ratios
- [ ] Images have appropriate alt text
- [ ] Forms have associated labels and error messages
- [ ] Dynamic content changes are announced to screen readers
- [ ] Heading hierarchy is correct and logical
- [ ] ARIA attributes are used correctly (not overused)
- [ ] Modal/dialog focus management works properly
- [ ] Touch targets are at least 44x44 CSS pixels on mobile
- [ ] Information is not conveyed by color alone
- [ ] Content is readable at 200% zoom

## Testing Accessibility

- **Automated**: Use `axe-core` via Playwright (`@axe-core/playwright`) in E2E tests
- **Manual keyboard testing**: Tab through the entire page, verify focus order
- **Screen reader testing**: Test with VoiceOver (macOS), NVDA (Windows), or Orca (Linux)
- **Browser DevTools**: Use Chrome's Accessibility panel to inspect the accessibility tree
