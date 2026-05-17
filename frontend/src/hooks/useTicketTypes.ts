import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { TicketType } from "@/types/event";

export function useTicketTypes(eventId: string | undefined) {
  return useQuery<TicketType[]>({
    queryKey: ["ticket-types", eventId],
    queryFn: () => api.get<TicketType[]>(`/events/${eventId}/ticket-types`),
    enabled: !!eventId,
  });
}
