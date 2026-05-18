# German Tax Calculation

## Description

Provides knowledge about German tax rules relevant to event ticketing, including VAT rates, accommodation tax, invoice requirements, and calculation methods for the DevConfTicketing application.

## German VAT (Umsatzsteuer / USt) Overview

Germany applies Value Added Tax (Umsatzsteuer) to goods and services. For event ticketing, the relevant rates are:

| Rate | Percentage | Applies To |
|------|-----------|------------|
| Standard rate | 19% | Conference tickets, workshop fees, catering, merchandise |
| Reduced rate | 7% | Accommodation services, books, certain cultural events |
| Exempt | 0% | Certain educational events (limited applicability) |

## Conference Ticket Taxation

Standard conference tickets (admission, talks, workshops) are taxed at the **19% standard VAT rate**.

### Calculation: Net → Gross

```
Net amount: €500.00
VAT (19%): €500.00 × 0.19 = €95.00
Gross amount: €500.00 + €95.00 = €595.00
```

### Calculation: Gross → Net

```
Gross amount: €595.00
Net amount: €595.00 / 1.19 = €500.00
VAT: €595.00 - €500.00 = €95.00
```

## Accommodation Tax (Beherbergungssteuer)

Many German cities levy a municipal **accommodation tax** (Beherbergungssteuer or Bettensteuer):

- This is a **municipal tax**, not subject to VAT
- Rate varies by city (typically 5% of net room rate or a fixed amount per night)
- **Important**: This is separate from VAT — it is added on top of the gross room rate
- Common cities with accommodation tax: Berlin (5%), Hamburg (varies), Munich (varies), Frankfurt (2€/night)

### Accommodation VAT

Hotel accommodation services are taxed at the **7% reduced VAT rate** (since 2010).

**Note**: Breakfast and other services included in the hotel rate are taxed at 19% standard rate and must be separated on the invoice.

```
Room rate (net): €120.00
VAT on room (7%): €120.00 × 0.07 = €8.40
Room gross: €128.40
Accommodation tax (Berlin 5%): €120.00 × 0.05 = €6.00
Total for accommodation: €128.40 + €6.00 = €134.40
```

## Catering (Verpflegungspauschale)

Catering and meal services at conferences are taxed at the **19% standard VAT rate**.

If offering a conference package that includes meals:
- The meal portion must be separated and taxed at 19%
- Or use the flat-rate meal allowance (Verpflegungspauschale) for business travelers:
  - Full day (24h absence): €28.00
  - Arrival/departure day: €14.00
  - These are tax-deductible amounts for the attendee's employer, not VAT amounts

## Invoice Requirements (§14 UStG)

German tax law requires invoices to include:

1. **Full name and address** of the seller (event organizer)
2. **Full name and address** of the buyer (attendee or company)
3. **Tax identification number** (Steuernummer) or **VAT ID** (USt-IdNr.) of the seller
4. **Sequential invoice number** — must be unique and sequential
5. **Invoice date** — date of issuance
6. **Delivery/service date** — date of the event
7. **Itemized line items** with:
   - Description of the service
   - Quantity
   - Net amount (Nettobetrag)
   - Tax rate (Steuersatz)
   - Tax amount (Steuerbetrag)
   - Gross amount (Bruttobetrag)
8. **Separate display of different tax rates** — if an order contains items at 19% and 7%, they must be grouped and totaled separately
9. **Total amounts** — net total, tax totals per rate, gross total

### Example Invoice Line Items

```
Conference Ticket "Full Access"    1x    €500.00    19%    €95.00    €595.00
Hotel Accommodation (2 nights)     1x    €240.00     7%    €16.80    €256.80
Conference Dinner                  1x     €65.00    19%    €12.35     €77.35
------------------------------------------------------------------------
                           Net 19%: €565.00    VAT 19%: €107.35
                           Net  7%: €240.00    VAT  7%:  €16.80
                           ----------------------------------------
                           Total Net:  €805.00
                           Total VAT:  €124.15
                           Total Gross: €929.15
```

## Implementation in DevConfTicketing

### TaxRate Model

Tax rates are stored in Cosmos DB in the `tax-rates` container with `/countryCode` as the partition key:

```csharp
public record TaxRate
{
    public required string Id { get; init; }
    public required string CountryCode { get; init; }  // "DE" for Germany
    public required decimal Percentage { get; init; }    // 19.0 or 7.0
    public required string Name { get; init; }           // "Standard VAT" or "Reduced VAT"
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}
```

### Tax Calculation Service

The `TaxCalculationService` in the Application layer handles tax computation:
- Accepts a net amount and a tax rate reference
- Computes tax amount and gross amount
- Rounds to 2 decimal places (standard for EUR)
- Supports multiple tax rates per order (grouped display)

### Order Line Items

Each `OrderLineItem` stores:
- `NetAmount` — price before tax
- `TaxRateId` — reference to the applicable tax rate
- `TaxRatePercentage` — snapshot of the rate at time of order (important for auditing)
- `TaxAmount` — computed tax
- `GrossAmount` — net + tax

### Rounding Rules

- All monetary amounts are in **EUR** (€) with **2 decimal places**
- Rounding: **half-up** (standard commercial rounding in Germany)
- Tax is calculated **per line item**, then summed — not calculated on the total
- Small rounding differences (±€0.01) may occur and are acceptable

## Reverse Charge Mechanism

If the buyer is a business in another EU country with a valid VAT ID:
- The reverse charge mechanism applies (§13b UStG)
- No VAT is charged on the invoice
- The invoice must state: "Steuerschuldnerschaft des Leistungsempfängers" (reverse charge)
- The buyer's VAT ID must be on the invoice
- Only applicable for B2B transactions within the EU

## Small Business Regulation (Kleinunternehmerregelung)

**Not applicable** for this project — the DevConfTicketing platform operates as a regular business subject to standard VAT rules (§19 UStG exemption is not claimed).
