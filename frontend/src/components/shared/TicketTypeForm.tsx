import { useState } from "react";
import { useForm, useWatch, type Resolver } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Separator } from "@/components/ui/separator";
import { LineItemEditor, type LineItemFormData } from "@/components/shared/LineItemEditor";
import type { TicketType, TaxRate } from "@/types/event";

const ticketTypeSchema = z.object({
  name: z.string().min(1, "Name is required"),
  description: z.string().optional().default(""),
  price: z.coerce.number().min(0, "Price must be non-negative"),
  currency: z.string().min(1).default("EUR"),
  availableQuantity: z.coerce.number().int().positive("Must be a positive number"),
  maxPerOrder: z.coerce.number().int().positive().default(10),
  showRemainingQuantity: z.boolean().default(false),
  saleStart: z.string().optional().or(z.literal("")),
  saleEnd: z.string().optional().or(z.literal("")),
});

type TicketTypeFormValues = z.infer<typeof ticketTypeSchema>;

interface TicketTypeFormProps {
  initialData?: TicketType;
  taxRates: TaxRate[];
  onSubmit: (data: TicketTypeFormValues & { lineItems: LineItemFormData[] }) => void;
  loading?: boolean;
  submitLabel?: string;
}

export function TicketTypeForm({
  initialData,
  taxRates,
  onSubmit,
  loading = false,
  submitLabel,
}: TicketTypeFormProps) {
  const form = useForm<TicketTypeFormValues>({
    resolver: zodResolver(ticketTypeSchema) as Resolver<TicketTypeFormValues>,
    defaultValues: {
      name: initialData?.name ?? "",
      description: initialData?.description ?? "",
      price: initialData?.price ?? 0,
      currency: initialData?.currency ?? "EUR",
      availableQuantity: initialData?.availableQuantity ?? 100,
      maxPerOrder: initialData?.maxPerOrder ?? 10,
      showRemainingQuantity: initialData?.showRemainingQuantity ?? false,
      saleStart: initialData?.saleStart ?? "",
      saleEnd: initialData?.saleEnd ?? "",
    },
  });

  const watchedCurrency = useWatch({ control: form.control, name: "currency" });

  const [lineItems, setLineItems] = useState<LineItemFormData[]>(
    initialData?.lineItems?.map((li) => ({
      name: li.name,
      netAmount: li.netAmount,
      taxRateId: li.taxRateId,
      taxRateName: li.taxRateName,
      taxRatePercentage: li.taxRatePercentage,
    })) ?? [],
  );

  const handleSubmit = (values: TicketTypeFormValues) => {
    onSubmit({
      ...values,
      saleStart: values.saleStart ? new Date(values.saleStart).toISOString() : undefined,
      saleEnd: values.saleEnd ? new Date(values.saleEnd).toISOString() : undefined,
      lineItems,
    });
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Name</FormLabel>
              <FormControl>
                <Input placeholder="Ticket type name" {...field} />
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

        <div className="grid gap-4 sm:grid-cols-3">
          <FormField
            control={form.control}
            name="price"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Price</FormLabel>
                <FormControl>
                  <Input type="number" step="0.01" min={0} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="currency"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Currency</FormLabel>
                <FormControl>
                  <Input placeholder="EUR" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="availableQuantity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Available Quantity</FormLabel>
                <FormControl>
                  <Input type="number" min={1} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="maxPerOrder"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Max Per Order</FormLabel>
                <FormControl>
                  <Input type="number" min={1} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="showRemainingQuantity"
            render={({ field }) => (
              <FormItem className="flex items-end gap-2 space-y-0 pb-2">
                <FormControl>
                  <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                </FormControl>
                <FormLabel>Show remaining quantity</FormLabel>
              </FormItem>
            )}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <FormField
            control={form.control}
            name="saleStart"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Sale Start</FormLabel>
                <FormControl>
                  <Input type="datetime-local" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="saleEnd"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Sale End</FormLabel>
                <FormControl>
                  <Input type="datetime-local" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <Separator />

        <LineItemEditor
          items={lineItems}
          onChange={setLineItems}
          taxRates={taxRates}
          currency={watchedCurrency || "EUR"}
        />

        <div className="flex justify-end gap-2">
          <Button type="submit" disabled={loading}>
            {loading ? "Saving…" : (submitLabel ?? (initialData ? "Update Ticket Type" : "Create Ticket Type"))}
          </Button>
        </div>
      </form>
    </Form>
  );
}
