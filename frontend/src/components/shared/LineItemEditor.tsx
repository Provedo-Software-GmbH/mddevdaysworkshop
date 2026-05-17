import { Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { TaxRate } from "@/types/event";
import { formatCurrency } from "@/components/shared/CurrencyDisplay";

export interface LineItemFormData {
  name: string;
  netAmount: number;
  taxRateId: string;
  taxRateName: string;
  taxRatePercentage: number;
}

interface LineItemEditorProps {
  items: LineItemFormData[];
  onChange: (items: LineItemFormData[]) => void;
  taxRates: TaxRate[];
  currency?: string;
}

export function LineItemEditor({ items, onChange, taxRates, currency = "EUR" }: LineItemEditorProps) {
  const addItem = () => {
    const defaultRate = taxRates.find((r) => r.isDefault && r.isActive) ?? taxRates[0];
    onChange([
      ...items,
      {
        name: "",
        netAmount: 0,
        taxRateId: defaultRate?.id ?? "",
        taxRateName: defaultRate?.name ?? "",
        taxRatePercentage: defaultRate?.percentage ?? 0,
      },
    ]);
  };

  const removeItem = (index: number) => {
    onChange(items.filter((_, i) => i !== index));
  };

  const updateItem = (index: number, updates: Partial<LineItemFormData>) => {
    onChange(
      items.map((item, i) => (i === index ? { ...item, ...updates } : item)),
    );
  };

  const handleTaxRateChange = (index: number, taxRateId: string) => {
    const rate = taxRates.find((r) => r.id === taxRateId);
    if (!rate) return;
    updateItem(index, {
      taxRateId: rate.id,
      taxRateName: rate.name,
      taxRatePercentage: rate.percentage,
    });
  };

  const totalGross = items.reduce((sum, item) => {
    const tax = (item.netAmount * item.taxRatePercentage) / 100;
    return sum + item.netAmount + tax;
  }, 0);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Label className="text-sm font-medium">Line Items</Label>
        <Button type="button" variant="outline" size="sm" onClick={addItem}>
          <Plus className="mr-1 h-4 w-4" />
          Add Line Item
        </Button>
      </div>

      {items.length === 0 && (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No line items yet. Add one to define the price breakdown.
        </p>
      )}

      {items.map((item, index) => {
        const tax = (item.netAmount * item.taxRatePercentage) / 100;
        const gross = item.netAmount + tax;

        return (
          <div key={index} className="rounded-md border p-4 space-y-3">
            <div className="flex items-start justify-between gap-2">
              <div className="flex-1">
                <Label className="text-xs">Name</Label>
                <Input
                  value={item.name}
                  onChange={(e) => updateItem(index, { name: e.target.value })}
                  placeholder="Line item name"
                />
              </div>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="mt-5 text-destructive"
                onClick={() => removeItem(index)}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>

            <div className="grid gap-3 sm:grid-cols-3">
              <div>
                <Label className="text-xs">Net Amount</Label>
                <Input
                  type="number"
                  step="0.01"
                  min={0}
                  value={item.netAmount}
                  onChange={(e) =>
                    updateItem(index, { netAmount: parseFloat(e.target.value) || 0 })
                  }
                />
              </div>
              <div>
                <Label className="text-xs">Tax Rate</Label>
                <Select
                  value={item.taxRateId}
                  onValueChange={(val) => handleTaxRateChange(index, val)}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select tax rate" />
                  </SelectTrigger>
                  <SelectContent>
                    {taxRates
                      .filter((r) => r.isActive)
                      .map((rate) => (
                        <SelectItem key={rate.id} value={rate.id}>
                          {rate.name} ({rate.percentage}%)
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="flex flex-col justify-end">
                <Label className="text-xs">Gross</Label>
                <p className="h-9 flex items-center text-sm font-medium tabular-nums">
                  {formatCurrency(gross, currency)}
                </p>
              </div>
            </div>
          </div>
        );
      })}

      {items.length > 0 && (
        <div className="flex justify-end text-sm font-medium">
          Total: {formatCurrency(totalGross, currency)}
        </div>
      )}
    </div>
  );
}
