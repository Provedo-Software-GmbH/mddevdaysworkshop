import { useState, useEffect, useMemo, useCallback } from "react";
import { Link, useParams, useLocation, useNavigate } from "react-router-dom";
import { format, parseISO } from "date-fns";
import { ArrowLeft, Minus, Plus, ShoppingCart, Ticket, Info, Loader2 } from "lucide-react";

import type { TicketType, LineItemTemplate } from "@/types/event";
import { useEvent } from "@/hooks/useEvents";
import { useTicketTypes } from "@/hooks/useTicketTypes";
import { useCreateOrder } from "@/hooks/useCreateOrder";
import { trackPageView } from "@/lib/telemetry";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Separator } from "@/components/ui/separator";

function formatCurrency(amount: number, currency = "EUR"): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency,
  }).format(amount);
}

function computeLineItemGross(item: LineItemTemplate) {
  const tax = (item.netAmount * item.taxRatePercentage) / 100;
  return { tax, gross: item.netAmount + tax };
}

function remainingQuantity(tt: TicketType): number {
  return tt.availableQuantity - tt.soldQuantity;
}

function maxSelectable(tt: TicketType): number {
  return Math.min(tt.maxPerOrder, remainingQuantity(tt));
}

export default function TicketSelectionPage() {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const navigate = useNavigate();
  const { data: event, isLoading: eventLoading, error: eventError } = useEvent(id);
  const { data: ticketTypes, isLoading: typesLoading, error: typesError } = useTicketTypes(id);
  const createOrder = useCreateOrder();

  const [selections, setSelections] = useState<Record<string, number>>({});

  useEffect(() => {
    trackPageView("ticket-selection", location.pathname);
  }, [location.pathname]);

  const updateQuantity = useCallback((ticketTypeId: string, delta: number, max: number) => {
    setSelections((prev) => {
      const current = prev[ticketTypeId] ?? 0;
      const next = Math.max(0, Math.min(max, current + delta));
      return { ...prev, [ticketTypeId]: next };
    });
  }, []);

  const selectedTypes = useMemo(() => {
    if (!ticketTypes) return [];
    return ticketTypes
      .filter((tt) => (selections[tt.id] ?? 0) > 0)
      .map((tt) => ({ ...tt, quantity: selections[tt.id] }));
  }, [ticketTypes, selections]);

  const grandTotal = useMemo(
    () => selectedTypes.reduce((sum, tt) => sum + tt.price * tt.quantity, 0),
    [selectedTypes],
  );

  const handleProceedToCheckout = useCallback(() => {
    if (!id || selectedTypes.length === 0) return;

    const positions = selectedTypes.map((tt) => ({
      ticketTypeId: tt.id,
      quantity: tt.quantity,
    }));

    createOrder.mutate(
      { eventId: id, request: { customerEmail: "", positions } },
      {
        onSuccess: (order) => {
          navigate(`/checkout?eventId=${id}&orderId=${order.id}`);
        },
      },
    );
  }, [id, selectedTypes, createOrder, navigate]);

  const isLoading = eventLoading || typesLoading;
  const error = eventError ?? typesError;

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8 space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-96" />
        <div className="space-y-4 mt-6">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-40 w-full" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-destructive text-lg font-medium">Failed to load ticket information.</p>
            <p className="text-muted-foreground mt-2">
              {error instanceof Error ? error.message : "An unexpected error occurred."}
            </p>
            <Button asChild variant="outline" className="mt-4">
              <Link to={`/events/${id}`}>
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to Event
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 space-y-6">
      {/* Back link & header */}
      <div>
        <Button asChild variant="ghost" size="sm" className="mb-2">
          <Link to={`/events/${id}`}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Event
          </Link>
        </Button>
        {event && (
          <>
            <h1 className="text-3xl font-bold tracking-tight">{event.title}</h1>
            <p className="text-muted-foreground mt-1">
              {format(parseISO(event.startDate), "PPP")} · {event.location}
            </p>
          </>
        )}
      </div>

      <Separator />

      {/* Empty state */}
      {(!ticketTypes || ticketTypes.length === 0) && (
        <Card>
          <CardContent className="py-12 text-center">
            <Ticket className="mx-auto h-12 w-12 text-muted-foreground" />
            <p className="mt-4 text-lg font-medium">No tickets available</p>
            <p className="text-muted-foreground mt-1">
              There are currently no ticket types for this event.
            </p>
          </CardContent>
        </Card>
      )}

      {/* Ticket type cards */}
      {ticketTypes && ticketTypes.length > 0 && (
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-4">
            <h2 className="text-xl font-semibold flex items-center gap-2">
              <Ticket className="h-5 w-5" />
              Available Tickets
            </h2>

            {ticketTypes.map((tt) => {
              const qty = selections[tt.id] ?? 0;
              const max = maxSelectable(tt);
              const remaining = remainingQuantity(tt);
              const subtotal = tt.price * qty;

              return (
                <Card key={tt.id}>
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between">
                      <div>
                        <CardTitle className="text-lg">{tt.name}</CardTitle>
                        {tt.description && (
                          <CardDescription className="mt-1">{tt.description}</CardDescription>
                        )}
                      </div>
                      <span className="text-xl font-bold whitespace-nowrap">
                        {formatCurrency(tt.price, tt.currency)}
                      </span>
                    </div>
                    {tt.showRemainingQuantity && (
                      <Badge variant={remaining > 10 ? "secondary" : "destructive"} className="w-fit mt-1">
                        {remaining} remaining
                      </Badge>
                    )}
                  </CardHeader>

                  <CardContent className="space-y-4">
                    {/* Line item breakdown */}
                    {tt.lineItems.length > 0 && (
                      <div className="rounded-md border p-3 text-sm space-y-2">
                        <p className="font-medium flex items-center gap-1 text-muted-foreground">
                          <Info className="h-3.5 w-3.5" />
                          Price Breakdown
                        </p>
                        {tt.lineItems.map((item, idx) => {
                          const { tax, gross } = computeLineItemGross(item);
                          return (
                            <div key={idx} className="flex items-center justify-between text-xs">
                              <div>
                                <span>{item.name}</span>
                                <span className="text-muted-foreground ml-2">
                                  ({item.taxRateName} {item.taxRatePercentage}%)
                                </span>
                              </div>
                              <div className="text-right tabular-nums space-x-3">
                                <span className="text-muted-foreground">
                                  Net {formatCurrency(item.netAmount, tt.currency)}
                                </span>
                                <span className="text-muted-foreground">
                                  Tax {formatCurrency(tax, tt.currency)}
                                </span>
                                <span className="font-medium">
                                  {formatCurrency(gross, tt.currency)}
                                </span>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}

                    {/* Quantity selector */}
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium">Quantity</span>
                      <div className="flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="icon"
                          className="h-8 w-8"
                          disabled={qty <= 0}
                          onClick={() => updateQuantity(tt.id, -1, max)}
                          aria-label={`Decrease ${tt.name} quantity`}
                        >
                          <Minus className="h-4 w-4" />
                        </Button>
                        <span className="w-10 text-center tabular-nums font-medium">{qty}</span>
                        <Button
                          variant="outline"
                          size="icon"
                          className="h-8 w-8"
                          disabled={qty >= max}
                          onClick={() => updateQuantity(tt.id, 1, max)}
                          aria-label={`Increase ${tt.name} quantity`}
                        >
                          <Plus className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </CardContent>

                  {qty > 0 && (
                    <CardFooter className="justify-end border-t pt-3">
                      <span className="text-sm">
                        Subtotal:{" "}
                        <span className="font-semibold">
                          {formatCurrency(subtotal, tt.currency)}
                        </span>
                      </span>
                    </CardFooter>
                  )}
                </Card>
              );
            })}
          </div>

          {/* Order summary sidebar */}
          <div className="lg:col-span-1">
            <div className="sticky top-4">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-lg">
                    <ShoppingCart className="h-5 w-5" />
                    Order Summary
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  {selectedTypes.length === 0 ? (
                    <p className="text-sm text-muted-foreground">
                      Select tickets to see your order summary.
                    </p>
                  ) : (
                    <>
                      {selectedTypes.map((tt) => (
                        <div key={tt.id} className="flex justify-between text-sm">
                          <span>
                            {tt.quantity}× {tt.name}
                          </span>
                          <span className="tabular-nums font-medium">
                            {formatCurrency(tt.price * tt.quantity, tt.currency)}
                          </span>
                        </div>
                      ))}
                      <Separator />
                      <div className="flex justify-between font-semibold">
                        <span>Total</span>
                        <span className="tabular-nums">
                          {formatCurrency(grandTotal, selectedTypes[0]?.currency)}
                        </span>
                      </div>
                    </>
                  )}
                </CardContent>
                <CardFooter>
                  <Button
                    className="w-full"
                    disabled={selectedTypes.length === 0 || createOrder.isPending}
                    onClick={handleProceedToCheckout}
                  >
                    {createOrder.isPending ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Creating order...
                      </>
                    ) : (
                      <>
                        <ShoppingCart className="mr-2 h-4 w-4" />
                        Proceed to Checkout
                      </>
                    )}
                  </Button>
                </CardFooter>
              </Card>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
