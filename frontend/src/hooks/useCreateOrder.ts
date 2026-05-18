import { useMutation } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { withSpan } from "@/lib/telemetry";
import type { CreateOrderRequest, Order } from "@/types/event";

interface CreateOrderParams {
  eventId: string;
  request: CreateOrderRequest;
}

export function useCreateOrder() {
  return useMutation<Order, Error, CreateOrderParams>({
    mutationFn: ({ eventId, request }) =>
      withSpan("checkout.createOrder", async (span) => {
        span.setAttribute("event.id", eventId);
        span.setAttribute("order.positions_count", request.positions.length);

        const order = await api.post<Order>(
          `/events/${eventId}/orders`,
          request,
        );

        span.setAttribute("order.id", order.id);
        span.setAttribute("order.code", order.orderCode);
        return order;
      }),
  });
}
