import type { LineItemTemplate } from "@/types/event";
import { formatCurrency } from "@/components/shared/CurrencyDisplay";

interface TaxBreakdownProps {
  lineItems: LineItemTemplate[];
  currency?: string;
}

export function TaxBreakdown({ lineItems, currency = "EUR" }: TaxBreakdownProps) {
  if (lineItems.length === 0) return null;

  const totals = lineItems.reduce(
    (acc, item) => {
      const tax = (item.netAmount * item.taxRatePercentage) / 100;
      return {
        net: acc.net + item.netAmount,
        tax: acc.tax + tax,
        gross: acc.gross + item.netAmount + tax,
      };
    },
    { net: 0, tax: 0, gross: 0 },
  );

  return (
    <div className="rounded-md border p-3 text-sm space-y-2">
      <p className="font-medium text-muted-foreground">Price Breakdown</p>
      {lineItems.map((item, idx) => {
        const tax = (item.netAmount * item.taxRatePercentage) / 100;
        const gross = item.netAmount + tax;
        return (
          <div key={idx} className="flex items-center justify-between text-xs">
            <div>
              <span>{item.name}</span>
              <span className="ml-2 text-muted-foreground">
                ({item.taxRateName} {item.taxRatePercentage}%)
              </span>
            </div>
            <div className="text-right tabular-nums space-x-3">
              <span className="text-muted-foreground">
                Net {formatCurrency(item.netAmount, currency)}
              </span>
              <span className="text-muted-foreground">
                Tax {formatCurrency(tax, currency)}
              </span>
              <span className="font-medium">{formatCurrency(gross, currency)}</span>
            </div>
          </div>
        );
      })}
      <div className="border-t pt-2 flex justify-between text-xs font-medium">
        <span>Total</span>
        <div className="text-right tabular-nums space-x-3">
          <span className="text-muted-foreground">Net {formatCurrency(totals.net, currency)}</span>
          <span className="text-muted-foreground">Tax {formatCurrency(totals.tax, currency)}</span>
          <span>{formatCurrency(totals.gross, currency)}</span>
        </div>
      </div>
    </div>
  );
}
