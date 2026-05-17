# Pretix Feature Comparison & Plan Adjustments

## Feature-by-Feature Comparison

This document compares our planned DevConf Ticketing features against pretix's feature set and identifies valuable additions.

### Legend
- ✅ = We have it / planned
- 🆕 = New — should add (high value for production use)
- 💡 = Nice-to-have — could be a future feature or workshop task
- ⏭️ = Skip — too complex or not relevant for our use case

---

## 1. Ticket Management

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| PDF ticket generation | ✅ Graphical layout editor, templates | ✅ Attendee 4 generates PDF | 🆕 **Upgrade**: Make PDF tickets a dedicated feature, not just email attachment |
| QR code per ticket | ✅ Unique cryptographic secret | ✅ Attendee 4 generates QR | ✅ Already planned |
| Multiple ticket layouts | ✅ Per-product layouts | ❌ | 💡 Future |
| Ticket download via API | ✅ `/orderpositions/{id}/download/pdf/` | ❌ | 🆕 **Add API endpoint** |
| Ticket download via web | ✅ Customer can download from order page | ❌ | 🆕 **Add to customer order detail** |
| Apple Wallet / Passbook | ✅ Plugin | ❌ | ⏭️ Future |
| Ticket validity periods | ✅ Fixed/dynamic validity | ❌ | ⏭️ Not needed for conferences |
| Revoked/blocked secrets | ✅ Separate models | ❌ | 🆕 **Add — essential for security** |

### Adjustments for Ticket Management

**Task 4 (Email & Notifications) → Rename to "Ticket PDF, Email & Notifications"**

Add to Task 4:
- Generate PDF tickets with QR code using QuestPDF
- Ticket download page at `/orders/{orderCode}/tickets`
- PDF download endpoint: `GET /api/v1/orders/{id}/tickets/pdf`
- Individual ticket download: `GET /api/v1/orders/{id}/tickets/{positionId}/pdf`
- The QR code should encode a unique, cryptographic ticket secret (not just the order ID)

---

## 2. Check-in / Access Control

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Check-in lists | ✅ Multiple per event, product filtering | ❌ | 🆕 **Add as new pre-built + attendee task** |
| Check-in API (scan) | ✅ Full RPC: redeem, search, annul | ❌ | 🆕 **Essential for production** |
| Web-based check-in | ✅ Browser-based scanning | ❌ | 🆕 **Add — no native app needed** |
| Multiple entry/exit | ✅ Entry + exit types | ❌ | 💡 Future |
| Check-in rules (JSON Logic) | ✅ Complex rules engine | ❌ | ⏭️ Over-engineered for our needs |
| Offline scanning | ✅ Mobile apps with local cache | ❌ | ⏭️ Not needed — always online |
| Auto check-in | ✅ Plugin | ❌ | ⏭️ |
| Check-in list CSV/PDF export | ✅ | ❌ | 🆕 **Add to admin** |
| Attendance certificates | ✅ PDF certificates | ❌ | 💡 Future |

### Adjustments for Check-in

**Replace Task 5 (Sessions & Speakers) → NEW: "Check-in & Ticket Scanning"**

Rationale: For a real ticketing system, check-in at the event is far more critical than session management. Sessions/speakers can be added later or managed externally.

New Task 5 scope:
- **Check-in List management** (admin): Create check-in lists per event, filter by ticket type
- **Check-in API**: `POST /api/v1/checkin/redeem` (scan QR → validate → mark checked in)
- **Check-in search**: `GET /api/v1/checkin/search?q=name` (find attendee by name)
- **Web-based check-in UI**: Mobile-friendly page for staff to scan QR codes (using browser camera)
- **Check-in statistics**: How many checked in vs. total
- **Check-in list export**: CSV download of all attendees with check-in status

---

## 3. Order Management

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Order lifecycle (pending→paid→cancelled) | ✅ | ✅ Basic | ✅ Already planned |
| Order cancellation | ✅ With cancellation fees | ❌ Only status change | 🆕 **Add cancellation with Stripe refund** |
| Order modification | ✅ Change product, attendee, etc. | ❌ | 💡 Future |
| Refunds | ✅ Full/partial, back to original method | ❌ | 🆕 **Add to Attendee 1 (Stripe)** |
| Multi-position orders | ✅ Different products per order | ❌ Single ticket type per order | 🆕 **Upgrade order model** |
| Attendee data per position | ✅ Name, email, custom fields | ❌ Only order-level customer | 🆕 **Add attendee info per ticket** |
| Order code (human-readable) | ✅ Short alphanumeric codes (e.g. `A7B2C`) | ❌ Using GUIDs | 🆕 **Add readable order codes** |
| Approval workflow | ✅ | ❌ | ⏭️ |
| Test mode | ✅ | ❌ | 💡 |

### Adjustments for Order Management

**Update pre-built Order model:**
- Add `OrderCode` (short, human-readable, e.g. `MDDD-A7K2`) — much better for customer support
- Support multiple ticket types per order (cart concept)
- Add `AttendeeInfo` per order position (name, email) — needed for personalized tickets
- Add cancellation support with Stripe refund integration (extend Attendee 1's scope)

**Update Order domain model:**
```
Order
├── OrderCode (human-readable, e.g. "MDDD-A7K2")
├── EventId
├── CustomerEmail
├── Status (Pending, Paid, Cancelled, Refunded)
├── Positions[]
│   ├── TicketTypeId
│   ├── TicketSecret (cryptographic, for QR code)
│   ├── AttendeeName
│   ├── AttendeeEmail
│   ├── LineItems[] (tax breakdown)
│   ├── CheckedInAt (nullable)
│   └── CheckedInBy (nullable)
├── TotalNet, TotalTax, TotalGross
├── PaymentInfo
└── CancellationDate (nullable)
```

---

## 4. Products / Items

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Products with tax-relevant line items | ✅ Tax rules per item | ✅ LineItemTemplate per ticket type | ✅ Already planned — our approach is better for German tax |
| Availability windows (sale start/end) | ✅ | ✅ SaleStart/SaleEnd on TicketType | ✅ Already planned |
| Quantity limits per order | ✅ min/max per order | ❌ | 🆕 **Add max per order** |
| Show remaining availability | ✅ `show_quota_left` | ❌ | 🆕 **Add — creates urgency** |
| Quotas (shared capacity) | ✅ Complex quota system | ✅ Simple: AvailableQuantity per TicketType | ✅ Sufficient |
| Add-ons | ✅ Add-on products | ❌ | 💡 Future (could model as line items) |
| Bundles | ✅ Product bundles | ❌ | ⏭️ |
| Custom attendee questions | ✅ Many field types | ❌ | 💡 Future |
| Categories | ✅ Product grouping | ❌ | 💡 Could add simply |
| Variations (sizes, tiers) | ✅ Per-product variations | ❌ | ⏭️ Not needed — use separate ticket types |

### Adjustments for Products

- Add `MaxPerOrder` to TicketType (e.g., max 5 tickets per order)
- Add `ShowRemainingQuantity` boolean to TicketType
- Display "Only X tickets left!" when remaining < threshold

---

## 5. Payments

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Stripe | ✅ | ✅ Attendee 1 | ✅ Already planned |
| Multiple payment providers | ✅ | ❌ Stripe only | ⏭️ Stripe is sufficient |
| Refunds | ✅ Full + partial | ❌ | 🆕 **Add to Attendee 1** |
| Payment fees/surcharges | ✅ | ❌ | ⏭️ |
| Gift cards | ✅ Cross-event balance | ❌ | ⏭️ |

### Adjustments for Payments

Add to Attendee 1 (Stripe Payment):
- `POST /api/v1/orders/{id}/refund` — Full refund via Stripe
- `POST /api/v1/orders/{id}/cancel` — Cancel order + trigger refund
- Handle `charge.refunded` webhook

---

## 6. Invoicing

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Auto-generated invoices | ✅ | ✅ Attendee 2 | ✅ Already planned |
| Sequential invoice numbers | ✅ | ✅ Attendee 2 | ✅ Already planned |
| Tax breakdown per rate | ✅ | ✅ Attendee 2 | ✅ Already planned |
| Cancellation invoices (Storno) | ✅ Auto-generated | ❌ | 🆕 **Add — legally required** |
| VAT ID validation | ✅ EU VAT check | ❌ | 💡 Future |
| ZUGFeRD e-invoice | ✅ Machine-readable | ❌ | 💡 Future |
| Invoice PDF download | ✅ | ✅ Attendee 2 | ✅ Already planned |

### Adjustments for Invoicing

Add to Attendee 2 (Invoice & Tax):
- Cancellation invoices (Stornorechnung) when an order is cancelled — legally required in Germany
- Invoice should reference the cancelled invoice number

---

## 7. Communication

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Order confirmation email | ✅ | ✅ Attendee 4 | ✅ Already planned |
| Ticket delivery email | ✅ | ✅ Attendee 4 | ✅ Already planned |
| Event reminder email | ✅ | ✅ Attendee 4 | ✅ Already planned |
| Bulk email to attendees | ✅ sendmail plugin | ❌ | 💡 Future |
| Waiting list | ✅ Auto-assign when available | ❌ | 🆕 **Add as pre-built domain model** |
| Email templates customizable | ✅ Full customization | ❌ | 💡 Future |

### Adjustments for Communication

- Add a `WaitingListEntry` domain model (pre-built) — useful when events sell out
- Pre-build the API: `POST /api/v1/events/{eventId}/waitlist` (register interest)
- Attendee 4 could extend: when ticket becomes available → notify waitlist

---

## 8. Vouchers / Discounts

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Voucher codes | ✅ Full system | ❌ | 🆕 **Add as pre-built domain model + simple API** |
| Percentage/absolute discounts | ✅ | ❌ | 🆕 (Part of voucher) |
| Voucher usage limits | ✅ | ❌ | 🆕 |
| Auto-discounts (rules) | ✅ | ❌ | ⏭️ Over-engineered |
| Early bird pricing | ✅ via availability windows | ✅ via SaleStart/SaleEnd + separate ticket type | ✅ Already possible |

### Adjustments for Vouchers

Add pre-built:
- `Voucher` domain model: `Code`, `DiscountType` (Percentage/Absolute/FixedPrice), `DiscountValue`, `MaxUsages`, `UsedCount`, `ValidUntil`, `ApplicableTicketTypeIds`
- `VoucherRepository` + basic CRUD API
- Voucher validation in order creation (pre-built skeleton)
- Attendees can extend: e.g., Attendee 1 applies voucher discount at Stripe checkout

---

## 9. Reporting / Exports

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Sales statistics dashboard | ✅ | ✅ Attendee 6 | ✅ Already planned |
| Attendee list export (CSV/XLSX) | ✅ | ❌ | 🆕 **Add to admin — essential** |
| Check-in list export | ✅ | ❌ | 🆕 **Add to check-in feature** |
| Financial/accounting report | ✅ PDF + Excel | ❌ | 🆕 **Add CSV export to Attendee 6** |
| Tax report by rate | ✅ | ✅ Attendee 2 has tax summary | ✅ Already planned |

### Adjustments for Reporting

Add to Attendee 6 (Dashboard & KPIs):
- `GET /api/v1/events/{eventId}/export/attendees` — CSV export of attendees
- `GET /api/v1/events/{eventId}/export/orders` — CSV export of orders
- Admin UI: "Export" buttons on attendee list and order list

---

## 10. Multi-Event / Organization

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Multiple events | ✅ | ✅ Core feature | ✅ Already planned |
| Organizer hierarchy | ✅ Teams, permissions | ❌ Single-tenant | ⏭️ Not needed — we are the only organizer |
| Event series (SubEvents) | ✅ | ❌ | ⏭️ Not needed initially |
| Event cloning | ✅ Clone via API | ❌ | 💡 Useful future feature |
| Team permissions | ✅ Fine-grained | ❌ | ⏭️ Use Entra ID roles |

---

## 11. Seating

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| Seating plans | ✅ Full editor + interactive maps | ❌ | ⏭️ Not needed for conferences |

---

## 12. API & Webhooks

| Feature | pretix | DevConf Ticketing | Action |
|---------|--------|-------------------|--------|
| REST API | ✅ Full CRUD | ✅ Minimal APIs | ✅ Already planned |
| Webhooks (outgoing) | ✅ Configurable per organizer | ❌ Only Stripe incoming | 💡 Future |
| API tokens | ✅ Team-scoped | ❌ | ⏭️ Use Entra ID |
| Idempotency | ✅ | ❌ | 💡 Good practice |

---

## Summary of Changes

### Pre-built Model Changes

1. **Order model upgrade**: Add `OrderCode`, `Positions[]` with `AttendeeInfo`, `TicketSecret`, `CheckedInAt`
2. **Voucher model** (new): Pre-built CRUD for discount codes
3. **WaitingListEntry model** (new): Pre-built skeleton
4. **TicketType additions**: `MaxPerOrder`, `ShowRemainingQuantity`

### Attendee Task Changes

| # | Original Task | Updated Task | Key Additions |
|---|---------------|--------------|---------------|
| 1 | Stripe Payment Integration | Stripe Payment, Refunds & Cancellations | + Refund API, + Cancel order, + Voucher discount at checkout |
| 2 | Invoice & Tax Calculation | Invoice, Tax & Cancellation Invoices | + Cancellation invoices (Stornorechnung) |
| 3 | Customer Auth & Account | Customer Auth & Account | No major changes |
| 4 | Email Notifications | **Ticket PDF, QR Codes & Email Notifications** | + PDF ticket generation, + Ticket download page, + Download API |
| 5 | Sessions & Speakers | **Check-in & Ticket Scanning** ⚠️ REPLACED | Completely new: check-in lists, scan API, web scanner UI |
| 6 | Admin Dashboard & KPIs | Dashboard, KPIs & Data Export | + CSV/XLSX export of attendees and orders |

### New Cosmos DB Containers

| Container | Partition Key | Description |
|-----------|--------------|-------------|
| `checkins` | `/eventId` | Check-in records |
| `vouchers` | `/eventId` | Discount vouchers |
| `waitlist` | `/eventId` | Waiting list entries |

*(Replaces `sessions` container — sessions/speakers removed)*

### New/Updated API Endpoints

```
# Check-in (🆕 Task 5)
POST   /api/v1/events/{eventId}/checkin/redeem      # Scan QR → check in
GET    /api/v1/events/{eventId}/checkin/search       # Find attendee
POST   /api/v1/events/{eventId}/checkin/annul        # Undo check-in
GET    /api/v1/events/{eventId}/checkin/stats        # Check-in statistics
GET    /api/v1/events/{eventId}/checkin/export       # CSV export

# Tickets / PDF (🆕 added to Task 4)
GET    /api/v1/orders/{id}/tickets/pdf               # Download all tickets as PDF
GET    /api/v1/orders/{id}/positions/{posId}/ticket   # Single ticket PDF

# Refunds (🆕 added to Task 1)
POST   /api/v1/orders/{id}/cancel                    # Cancel + refund
POST   /api/v1/orders/{id}/refund                    # Manual refund

# Vouchers (🆕 pre-built)
GET    /api/v1/events/{eventId}/vouchers
POST   /api/v1/events/{eventId}/vouchers
PUT    /api/v1/events/{eventId}/vouchers/{id}
DELETE /api/v1/events/{eventId}/vouchers/{id}
POST   /api/v1/vouchers/validate                     # Validate code at checkout

# Exports (🆕 added to Task 6)
GET    /api/v1/events/{eventId}/export/attendees     # CSV
GET    /api/v1/events/{eventId}/export/orders        # CSV
```
