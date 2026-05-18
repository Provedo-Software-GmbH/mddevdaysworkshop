import { useMutation } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { withSpan } from "@/lib/telemetry";
import type { CheckoutRequest, CheckoutResponse } from "@/types/event";

interface InitiateCheckoutParams {
  eventId: string;
  orderId: string;
  successUrl: string;
  cancelUrl: string;
}

export function useCheckout() {
  return useMutation<CheckoutResponse, Error, InitiateCheckoutParams>({
    mutationFn: ({ eventId, orderId, successUrl, cancelUrl }) =>
      withSpan("checkout.start", async (span) => {
        span.setAttribute("order.id", orderId);
        span.setAttribute("event.id", eventId);

        const request: CheckoutRequest = { successUrl, cancelUrl };
        const response = await api.post<CheckoutResponse>(
          `/events/${eventId}/orders/${orderId}/checkout`,
          request,
        );

        span.setAttribute("stripe.session_id", response.sessionId);
        return response;
      }),
  });
}
