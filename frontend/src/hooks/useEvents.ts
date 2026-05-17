import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { Event } from "@/types/event";

export function useEvents() {
  return useQuery<Event[]>({
    queryKey: ["events"],
    queryFn: () => api.get<Event[]>("/events"),
  });
}

export function useEvent(id: string | undefined) {
  return useQuery<Event>({
    queryKey: ["events", id],
    queryFn: () => api.get<Event>(`/events/${id}`),
    enabled: !!id,
  });
}

export interface CreateEventData {
  title: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string;
  organizerId: string;
  maxAttendees: number;
  imageUrl?: string | null;
  websiteUrl?: string | null;
}

export type UpdateEventData = CreateEventData;

export function useCreateEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateEventData) =>
      api.post<Event>("/events", data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
}

export function useUpdateEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateEventData }) =>
      api.put<Event>(`/events/${id}`, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["events", variables.id] });
    },
  });
}

export function usePublishEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      api.post<Event>(`/events/${id}/publish`),
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
      queryClient.invalidateQueries({ queryKey: ["events", id] });
    },
  });
}

export function useDeleteEvent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      api.delete<void>(`/events/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["events"] });
    },
  });
}
