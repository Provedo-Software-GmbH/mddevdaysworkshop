import { cn } from "@/lib/utils";

interface CurrencyDisplayProps {
  amount: number;
  currency?: string;
  className?: string;
}

export function formatCurrency(amount: number, currency = "EUR"): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency,
  }).format(amount);
}

export function CurrencyDisplay({ amount, currency = "EUR", className }: CurrencyDisplayProps) {
  return (
    <span className={cn("tabular-nums", className)}>
      {formatCurrency(amount, currency)}
    </span>
  );
}
