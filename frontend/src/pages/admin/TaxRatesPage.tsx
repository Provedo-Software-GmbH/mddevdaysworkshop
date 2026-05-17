import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function TaxRatesPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Tax Rates</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Tax Rates</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">
            Tax rate management will be implemented in Step 8.
          </p>
          <div className="mt-4 space-y-3">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
