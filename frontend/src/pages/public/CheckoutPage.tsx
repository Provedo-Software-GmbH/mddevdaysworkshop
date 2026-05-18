import { useState, useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { ArrowLeft, CreditCard, Mail, Loader2, ShoppingCart } from "lucide-react";

import { useOrder } from "@/hooks/useOrders";
import { useCheckout } from "@/hooks/useCheckout";
import { trackPageView, startSpan, trackError } from "@/lib/telemetry";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";

function formatCurrency(amount: number, currency = "EUR"): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency,
  }).format(amount);
}

export default function CheckoutPage() {
  const [searchParams] = useSearchParams();

  const eventId = searchParams.get("eventId");
  const orderId = searchParams.get("orderId");

  const { data: order, isLoading, error } = useOrder(eventId ?? undefined, orderId ?? undefined);
  const checkout = useCheckout();

  const [email, setEmail] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);

  useEffect(() => {
    trackPageView("checkout", "/checkout");
  }, []);

  useEffect(() => {
    if (order?.customerEmail && !email) {
      setEmail(order.customerEmail);
    }
  }, [order?.customerEmail]); // eslint-disable-line react-hooks/exhaustive-deps

  function validateEmail(value: string): boolean {
    if (!value.trim()) {
      setEmailError("Email address is required.");
      return false;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
      setEmailError("Please enter a valid email address.");
      return false;
    }
    setEmailError(null);
    return true;
  }

  async function handleCheckout() {
    if (!validateEmail(email)) return;
    if (!eventId || !orderId) return;

    const successUrl = `${window.location.origin}/checkout/success?eventId=${eventId}&orderId=${orderId}`;
    const cancelUrl = `${window.location.origin}/checkout/cancel?eventId=${eventId}&orderId=${orderId}`;

    checkout.mutate(
      { eventId, orderId, successUrl, cancelUrl },
      {
        onSuccess: (response) => {
          const span = startSpan("checkout.redirect", {
            "stripe.session_url": response.sessionUrl,
          });
          window.location.href = response.sessionUrl;
          span.end();
        },
        onError: (err) => {
          const span = startSpan("checkout.error");
          trackError(span, err);
          span.end();
        },
      },
    );
  }

  if (!eventId || !orderId) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-destructive text-lg font-medium">Invalid checkout link.</p>
            <p className="text-muted-foreground mt-2">
              Missing order or event information. Please start from the event page.
            </p>
            <Button asChild variant="outline" className="mt-4">
              <Link to="/">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to Events
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8 space-y-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-96" />
        <div className="space-y-4 mt-6">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-destructive text-lg font-medium">Failed to load order.</p>
            <p className="text-muted-foreground mt-2">
              {error instanceof Error ? error.message : "An unexpected error occurred."}
            </p>
            <Button asChild variant="outline" className="mt-4">
              <Link to="/">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Back to Events
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8 space-y-6 max-w-3xl">
      {/* Header */}
      <div>
        <Button asChild variant="ghost" size="sm" className="mb-2">
          <Link to={`/events/${eventId}/tickets`}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Tickets
          </Link>
        </Button>
        <h1 className="text-3xl font-bold tracking-tight">Checkout</h1>
        <p className="text-muted-foreground mt-1">
          Order #{order.orderCode}
        </p>
      </div>

      <Separator />

      {/* Order Summary */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <ShoppingCart className="h-5 w-5" />
            Order Summary
          </CardTitle>
          <CardDescription>Review your order before payment</CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {order.positions.map((position, idx) => (
            <div key={idx} className="flex justify-between text-sm">
              <span>{position.ticketTypeName}</span>
              <span className="tabular-nums text-muted-foreground">1×</span>
            </div>
          ))}

          <Separator />

          <div className="space-y-1 text-sm">
            <div className="flex justify-between text-muted-foreground">
              <span>Subtotal (net)</span>
              <span className="tabular-nums">{formatCurrency(order.totalNet, order.currency)}</span>
            </div>
            <div className="flex justify-between text-muted-foreground">
              <span>Tax</span>
              <span className="tabular-nums">{formatCurrency(order.totalTax, order.currency)}</span>
            </div>
            {order.discountAmount > 0 && (
              <div className="flex justify-between text-green-600">
                <span>Discount</span>
                <span className="tabular-nums">-{formatCurrency(order.discountAmount, order.currency)}</span>
              </div>
            )}
          </div>

          <Separator />

          <div className="flex justify-between font-semibold text-lg">
            <span>Total</span>
            <span className="tabular-nums">{formatCurrency(order.totalGross, order.currency)}</span>
          </div>
        </CardContent>
      </Card>

      {/* Email Input */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-lg">
            <Mail className="h-5 w-5" />
            Contact Information
          </CardTitle>
          <CardDescription>We'll send your tickets to this email address</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <Label htmlFor="email">Email address</Label>
            <Input
              id="email"
              type="email"
              placeholder="your@email.com"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                if (emailError) validateEmail(e.target.value);
              }}
              onBlur={() => validateEmail(email)}
              aria-invalid={!!emailError}
              aria-describedby={emailError ? "email-error" : undefined}
            />
            {emailError && (
              <p id="email-error" className="text-sm text-destructive" role="alert">
                {emailError}
              </p>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Pay Button */}
      <Card>
        <CardFooter className="pt-6">
          <Button
            className="w-full"
            size="lg"
            onClick={handleCheckout}
            disabled={checkout.isPending}
          >
            {checkout.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Redirecting to payment...
              </>
            ) : (
              <>
                <CreditCard className="mr-2 h-4 w-4" />
                Pay Now — {formatCurrency(order.totalGross, order.currency)}
              </>
            )}
          </Button>
        </CardFooter>
        {checkout.isError && (
          <CardContent>
            <p className="text-sm text-destructive text-center" role="alert">
              Payment initiation failed. Please try again.
            </p>
          </CardContent>
        )}
      </Card>
    </div>
  );
}
