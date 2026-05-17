import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { useEvent, useUpdateEvent } from "@/hooks/useEvents";
import { withSpan } from "@/lib/telemetry";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { EventForm } from "@/components/shared/EventForm";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";

export default function EditEventPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: event, isLoading } = useEvent(id);
  const updateEvent = useUpdateEvent();

  const handleSubmit = async (data: Parameters<typeof updateEvent.mutateAsync>[0]["data"]) => {
    if (!id) return;
    try {
      await withSpan("admin.event.update", async (span) => {
        span.setAttribute("event.id", id);
        await updateEvent.mutateAsync({ id, data });
      });
      toast.success("Event updated successfully.");
      navigate("/admin/events");
    } catch {
      toast.error("Failed to update event.");
    }
  };

  if (isLoading) {
    return <LoadingSpinner className="py-16" label="Loading event…" />;
  }

  if (!event) {
    return (
      <div className="py-16 text-center">
        <p className="text-lg font-medium text-muted-foreground">Event not found.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate("/admin/events")}>
          Back to Events
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/admin/events")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-3xl font-bold tracking-tight">Edit Event</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Event Details</CardTitle>
        </CardHeader>
        <CardContent>
          <EventForm
            initialData={event}
            onSubmit={handleSubmit}
            loading={updateEvent.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
