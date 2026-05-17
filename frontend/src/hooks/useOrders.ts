import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { Order } from "@/types/event";

export function useOrders(eventId: string | undefined) {
  return useQuery<Order[]>({
    queryKey: ["orders", eventId],
    queryFn: () => api.get<Order[]>(`/events/${eventId}/orders`),
    enabled: !!eventId,
  });
}

export function useOrder(eventId: string | undefined, orderId: string | undefined) {
  return useQuery<Order>({
    queryKey: ["orders", eventId, orderId],
    queryFn: () => api.get<Order>(`/events/${eventId}/orders/${orderId}`),
    enabled: !!eventId && !!orderId,
  });
}
