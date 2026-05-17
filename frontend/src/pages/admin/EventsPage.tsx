import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { format, parseISO } from "date-fns";
import { Plus, Pencil, Trash2, Send } from "lucide-react";
import { toast } from "sonner";

import { useEvents, useDeleteEvent, usePublishEvent } from "@/hooks/useEvents";
import { withSpan } from "@/lib/telemetry";
import type { Event } from "@/types/event";

import { Button } from "@/components/ui/button";
import { DataTable, type Column } from "@/components/shared/DataTable";
import { EventStatusBadge } from "@/components/shared/EventStatusBadge";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/shared/EmptyState";
import { ConfirmDialog } from "@/components/shared/ConfirmDialog";

export default function EventsPage() {
  const navigate = useNavigate();
  const { data: events, isLoading } = useEvents();
  const deleteEvent = useDeleteEvent();
  const publishEvent = usePublishEvent();

  const [deleteTarget, setDeleteTarget] = useState<Event | null>(null);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await withSpan("admin.event.delete", async (span) => {
        span.setAttribute("event.id", deleteTarget.id);
        await deleteEvent.mutateAsync(deleteTarget.id);
      });
      toast.success("Event deleted successfully.");
      setDeleteTarget(null);
    } catch {
      toast.error("Failed to delete event.");
    }
  };

  const handlePublish = async (event: Event) => {
    try {
      await withSpan("admin.event.publish", async (span) => {
        span.setAttribute("event.id", event.id);
        await publishEvent.mutateAsync(event.id);
      });
      toast.success(`"${event.title}" published successfully.`);
    } catch {
      toast.error("Failed to publish event.");
    }
  };

  const columns: Column<Event>[] = [
    {
      key: "title",
      header: "Title",
      sortable: true,
      render: (e) => <span className="font-medium">{e.title}</span>,
    },
    {
      key: "location",
      header: "Location",
      sortable: true,
      render: (e) => e.location,
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (e) => <EventStatusBadge status={e.status} />,
    },
    {
      key: "startDate",
      header: "Start Date",
      sortable: true,
      render: (e) => format(parseISO(e.startDate), "PPP"),
    },
    {
      key: "actions",
      header: "Actions",
      render: (e) => (
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            onClick={(ev) => {
              ev.stopPropagation();
              navigate(`/admin/events/${e.id}/edit`);
            }}
          >
            <Pencil className="h-4 w-4" />
          </Button>
          {e.status === "Draft" && (
            <Button
              variant="ghost"
              size="icon"
              onClick={(ev) => {
                ev.stopPropagation();
                handlePublish(e);
              }}
            >
              <Send className="h-4 w-4" />
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            className="text-destructive"
            onClick={(ev) => {
              ev.stopPropagation();
              setDeleteTarget(e);
            }}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  if (isLoading) {
    return <LoadingSpinner className="py-16" label="Loading events…" />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Events</h1>
        <Button onClick={() => navigate("/admin/events/new")}>
          <Plus className="mr-2 h-4 w-4" />
          Create Event
        </Button>
      </div>

      {!events || events.length === 0 ? (
        <EmptyState
          title="No events yet"
          description="Create your first event to get started."
          action={
            <Button onClick={() => navigate("/admin/events/new")}>
              <Plus className="mr-2 h-4 w-4" />
              Create Event
            </Button>
          }
        />
      ) : (
        <DataTable
          data={events}
          columns={columns}
          keyFn={(e) => e.id}
          onRowClick={(e) => navigate(`/admin/events/${e.id}/edit`)}
        />
      )}

      <ConfirmDialog
        open={!!deleteTarget}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        title="Delete Event"
        description={`Are you sure you want to delete "${deleteTarget?.title}"? This action cannot be undone.`}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        loading={deleteEvent.isPending}
      />
    </div>
  );
}
