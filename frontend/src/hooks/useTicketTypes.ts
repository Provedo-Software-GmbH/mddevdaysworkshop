import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { TicketType, LineItemTemplate } from "@/types/event";

export function useTicketTypes(eventId: string | undefined) {
  return useQuery<TicketType[]>({
    queryKey: ["ticket-types", eventId],
    queryFn: () => api.get<TicketType[]>(`/events/${eventId}/ticket-types`),
    enabled: !!eventId,
  });
}

export interface CreateTicketTypeData {
  name: string;
  description: string;
  price: number;
  currency: string;
  availableQuantity: number;
  lineItems: LineItemTemplate[];
  maxPerOrder?: number;
  showRemainingQuantity?: boolean;
  saleStart?: string | null;
  saleEnd?: string | null;
}

export type UpdateTicketTypeData = CreateTicketTypeData;

export function useCreateTicketType() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ eventId, data }: { eventId: string; data: CreateTicketTypeData }) =>
      api.post<TicketType>(`/events/${eventId}/ticket-types`, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["ticket-types", variables.eventId] });
    },
  });
}

export function useUpdateTicketType() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      eventId,
      id,
      data,
    }: {
      eventId: string;
      id: string;
      data: UpdateTicketTypeData;
    }) => api.put<TicketType>(`/events/${eventId}/ticket-types/${id}`, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["ticket-types", variables.eventId] });
    },
  });
}

export function useDeleteTicketType() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ eventId, id }: { eventId: string; id: string }) =>
      api.delete<void>(`/events/${eventId}/ticket-types/${id}`),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["ticket-types", variables.eventId] });
    },
  });
}
