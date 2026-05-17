import { useNavigate } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";

import { useCreateEvent } from "@/hooks/useEvents";
import { withSpan } from "@/lib/telemetry";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { EventForm } from "@/components/shared/EventForm";

export default function CreateEventPage() {
  const navigate = useNavigate();
  const createEvent = useCreateEvent();

  const handleSubmit = async (data: Parameters<typeof createEvent.mutateAsync>[0]) => {
    try {
      await withSpan("admin.event.create", async (span) => {
        span.setAttribute("event.title", data.title);
        await createEvent.mutateAsync(data);
      });
      toast.success("Event created successfully.");
      navigate("/admin/events");
    } catch {
      toast.error("Failed to create event.");
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/admin/events")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-3xl font-bold tracking-tight">Create Event</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Event Details</CardTitle>
        </CardHeader>
        <CardContent>
          <EventForm onSubmit={handleSubmit} loading={createEvent.isPending} />
        </CardContent>
      </Card>
    </div>
  );
}
