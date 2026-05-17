import { useState } from "react";
import { useForm, type Resolver } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Plus, Pencil } from "lucide-react";
import { toast } from "sonner";

import { useTaxRates, useCreateTaxRate, useUpdateTaxRate } from "@/hooks/useTaxRates";
import { withSpan } from "@/lib/telemetry";
import type { TaxRate } from "@/types/event";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Badge } from "@/components/ui/badge";
import { DataTable, type Column } from "@/components/shared/DataTable";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/shared/EmptyState";

const taxRateSchema = z.object({
  countryCode: z.string().min(2, "Country code is required").max(5),
  name: z.string().min(1, "Name is required"),
  percentage: z.coerce.number().min(0).max(100),
  description: z.string().optional().default(""),
  isDefault: z.boolean().default(false),
  isActive: z.boolean().default(true),
});

type TaxRateFormValues = z.infer<typeof taxRateSchema>;

export default function TaxRatesPage() {
  const { data: taxRates, isLoading } = useTaxRates();
  const createTaxRate = useCreateTaxRate();
  const updateTaxRate = useUpdateTaxRate();

  const [formOpen, setFormOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<TaxRate | null>(null);

  const form = useForm<TaxRateFormValues>({
    resolver: zodResolver(taxRateSchema) as Resolver<TaxRateFormValues>,
    defaultValues: {
      countryCode: "DE",
      name: "",
      percentage: 19,
      description: "",
      isDefault: false,
      isActive: true,
    },
  });

  const openCreate = () => {
    form.reset({
      countryCode: "DE",
      name: "",
      percentage: 19,
      description: "",
      isDefault: false,
      isActive: true,
    });
    setEditTarget(null);
    setFormOpen(true);
  };

  const openEdit = (taxRate: TaxRate) => {
    form.reset({
      countryCode: taxRate.countryCode,
      name: taxRate.name,
      percentage: taxRate.percentage,
      description: taxRate.description,
      isDefault: taxRate.isDefault,
      isActive: taxRate.isActive,
    });
    setEditTarget(taxRate);
    setFormOpen(true);
  };

  const handleSubmit = async (values: TaxRateFormValues) => {
    try {
      if (editTarget) {
        await withSpan("admin.taxRate.update", async (span) => {
          span.setAttribute("taxRate.id", editTarget.id);
          await updateTaxRate.mutateAsync({
            countryCode: editTarget.countryCode,
            id: editTarget.id,
            data: {
              name: values.name,
              percentage: values.percentage,
              description: values.description ?? "",
              isDefault: values.isDefault,
              isActive: values.isActive,
            },
          });
        });
        toast.success("Tax rate updated.");
      } else {
        await withSpan("admin.taxRate.create", async (span) => {
          span.setAttribute("taxRate.name", values.name);
          await createTaxRate.mutateAsync({
            countryCode: values.countryCode,
            name: values.name,
            percentage: values.percentage,
            description: values.description ?? "",
            isDefault: values.isDefault,
            isActive: values.isActive,
          });
        });
        toast.success("Tax rate created.");
      }
      setFormOpen(false);
    } catch {
      toast.error(editTarget ? "Failed to update tax rate." : "Failed to create tax rate.");
    }
  };

  const columns: Column<TaxRate>[] = [
    {
      key: "name",
      header: "Name",
      sortable: true,
      render: (r) => <span className="font-medium">{r.name}</span>,
    },
    {
      key: "countryCode",
      header: "Country",
      sortable: true,
      render: (r) => r.countryCode,
    },
    {
      key: "percentage",
      header: "Rate",
      sortable: true,
      render: (r) => `${r.percentage}%`,
    },
    {
      key: "status",
      header: "Status",
      render: (r) => (
        <div className="flex gap-1">
          {r.isActive ? (
            <Badge variant="default">Active</Badge>
          ) : (
            <Badge variant="secondary">Inactive</Badge>
          )}
          {r.isDefault && <Badge variant="outline">Default</Badge>}
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      render: (r) => (
        <Button variant="ghost" size="icon" onClick={() => openEdit(r)}>
          <Pencil className="h-4 w-4" />
        </Button>
      ),
    },
  ];

  if (isLoading) {
    return <LoadingSpinner className="py-16" label="Loading tax rates…" />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Tax Rates</h1>
        <Button onClick={openCreate}>
          <Plus className="mr-2 h-4 w-4" />
          Add Tax Rate
        </Button>
      </div>

      {!taxRates || taxRates.length === 0 ? (
        <EmptyState
          title="No tax rates"
          description="Add tax rates to assign to ticket type line items."
          action={
            <Button onClick={openCreate}>
              <Plus className="mr-2 h-4 w-4" />
              Add Tax Rate
            </Button>
          }
        />
      ) : (
        <DataTable data={taxRates} columns={columns} keyFn={(r) => r.id} />
      )}

      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editTarget ? "Edit Tax Rate" : "Create Tax Rate"}</DialogTitle>
          </DialogHeader>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="countryCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Country Code</FormLabel>
                    <FormControl>
                      <Input placeholder="DE" {...field} disabled={!!editTarget} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Name</FormLabel>
                    <FormControl>
                      <Input placeholder="Standard Rate" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="percentage"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Percentage</FormLabel>
                    <FormControl>
                      <Input type="number" step="0.01" min={0} max={100} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Textarea placeholder="Optional description" rows={2} {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="flex gap-6">
                <FormField
                  control={form.control}
                  name="isDefault"
                  render={({ field }) => (
                    <FormItem className="flex items-center gap-2 space-y-0">
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                      <FormLabel>Default</FormLabel>
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="isActive"
                  render={({ field }) => (
                    <FormItem className="flex items-center gap-2 space-y-0">
                      <FormControl>
                        <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                      </FormControl>
                      <FormLabel>Active</FormLabel>
                    </FormItem>
                  )}
                />
              </div>
              <div className="flex justify-end">
                <Button type="submit" disabled={createTaxRate.isPending || updateTaxRate.isPending}>
                  {createTaxRate.isPending || updateTaxRate.isPending
                    ? "Saving…"
                    : editTarget
                      ? "Update"
                      : "Create"}
                </Button>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
