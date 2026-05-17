import { Badge } from "@/components/ui/badge";
import type { EventStatus } from "@/types/event";

const statusConfig: Record<EventStatus, { variant: "default" | "secondary" | "destructive" | "outline"; label: string }> = {
  Draft: { variant: "secondary", label: "Draft" },
  Published: { variant: "default", label: "Published" },
  Cancelled: { variant: "destructive", label: "Cancelled" },
  Archived: { variant: "outline", label: "Archived" },
};

interface EventStatusBadgeProps {
  status: EventStatus;
}

export function EventStatusBadge({ status }: EventStatusBadgeProps) {
  const config = statusConfig[status];
  return <Badge variant={config.variant}>{config.label}</Badge>;
}
