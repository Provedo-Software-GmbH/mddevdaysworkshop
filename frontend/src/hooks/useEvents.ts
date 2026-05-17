import { useQuery } from "@tanstack/react-query";
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
