import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Pencil, Trash2 } from "lucide-react";
import { toast } from "sonner";

import {
  useTicketTypes,
  useCreateTicketType,
  useUpdateTicketType,
  useDeleteTicketType,
} from "@/hooks/useTicketTypes";
import { useTaxRates } from "@/hooks/useTaxRates";
import { useEvent } from "@/hooks/useEvents";
import { withSpan } from "@/lib/telemetry";
import type { TicketType } from "@/types/event";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { DataTable, type Column } from "@/components/shared/DataTable";
import { CurrencyDisplay } from "@/components/shared/CurrencyDisplay";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/shared/EmptyState";
import { ConfirmDialog } from "@/components/shared/ConfirmDialog";
import { TicketTypeForm } from "@/components/shared/TicketTypeForm";

export default function TicketTypesPage() {
  const { id: eventId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: event } = useEvent(eventId);
  const { data: ticketTypes, isLoading } = useTicketTypes(eventId);
  const { data: taxRates } = useTaxRates();
  const createTicketType = useCreateTicketType();
  const updateTicketType = useUpdateTicketType();
  const deleteTicketType = useDeleteTicketType();

  const [formOpen, setFormOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<TicketType | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<TicketType | null>(null);

  const handleCreate = async (data: Parameters<typeof createTicketType.mutateAsync>[0]["data"]) => {
    if (!eventId) return;
    try {
      await withSpan("admin.ticketType.create", async (span) => {
        span.setAttribute("event.id", eventId);
        span.setAttribute("ticketType.name", data.name);
        await createTicketType.mutateAsync({ eventId, data });
      });
      toast.success("Ticket type created.");
      setFormOpen(false);
    } catch {
      toast.error("Failed to create ticket type.");
    }
  };

  const handleUpdate = async (data: Parameters<typeof updateTicketType.mutateAsync>[0]["data"]) => {
    if (!eventId || !editTarget) return;
    try {
      await withSpan("admin.ticketType.update", async (span) => {
        span.setAttribute("event.id", eventId);
        span.setAttribute("ticketType.id", editTarget.id);
        await updateTicketType.mutateAsync({ eventId, id: editTarget.id, data });
      });
      toast.success("Ticket type updated.");
      setEditTarget(null);
    } catch {
      toast.error("Failed to update ticket type.");
    }
  };

  const handleDelete = async () => {
    if (!eventId || !deleteTarget) return;
    try {
      await withSpan("admin.ticketType.delete", async (span) => {
        span.setAttribute("event.id", eventId);
        span.setAttribute("ticketType.id", deleteTarget.id);
        await deleteTicketType.mutateAsync({ eventId, id: deleteTarget.id });
      });
      toast.success("Ticket type deleted.");
      setDeleteTarget(null);
    } catch {
      toast.error("Failed to delete ticket type.");
    }
  };

  const columns: Column<TicketType>[] = [
    {
      key: "name",
      header: "Name",
      sortable: true,
      render: (tt) => <span className="font-medium">{tt.name}</span>,
    },
    {
      key: "price",
      header: "Price",
      sortable: true,
      render: (tt) => <CurrencyDisplay amount={tt.price} currency={tt.currency} />,
    },
    {
      key: "available",
      header: "Available",
      sortable: true,
      render: (tt) => `${tt.availableQuantity - tt.soldQuantity} / ${tt.availableQuantity}`,
    },
    {
      key: "maxPerOrder",
      header: "Max/Order",
      render: (tt) => tt.maxPerOrder,
    },
    {
      key: "actions",
      header: "Actions",
      render: (tt) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" onClick={() => setEditTarget(tt)}>
            <Pencil className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="text-destructive"
            onClick={() => setDeleteTarget(tt)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  if (isLoading) {
    return <LoadingSpinner className="py-16" label="Loading ticket types…" />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/admin/events")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Ticket Types</h1>
          {event && (
            <p className="text-muted-foreground">{event.title}</p>
          )}
        </div>
      </div>

      <div className="flex justify-end">
        <Button onClick={() => setFormOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Add Ticket Type
        </Button>
      </div>

      {!ticketTypes || ticketTypes.length === 0 ? (
        <EmptyState
          title="No ticket types"
          description="Add ticket types for attendees to purchase."
          action={
            <Button onClick={() => setFormOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Add Ticket Type
            </Button>
          }
        />
      ) : (
        <DataTable data={ticketTypes} columns={columns} keyFn={(tt) => tt.id} />
      )}

      {/* Create dialog */}
      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Create Ticket Type</DialogTitle>
          </DialogHeader>
          <TicketTypeForm
            taxRates={taxRates ?? []}
            onSubmit={handleCreate}
            loading={createTicketType.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Edit dialog */}
      <Dialog open={!!editTarget} onOpenChange={(open) => !open && setEditTarget(null)}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit Ticket Type</DialogTitle>
          </DialogHeader>
          {editTarget && (
            <TicketTypeForm
              initialData={editTarget}
              taxRates={taxRates ?? []}
              onSubmit={handleUpdate}
              loading={updateTicketType.isPending}
            />
          )}
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        title="Delete Ticket Type"
        description={`Are you sure you want to delete "${deleteTarget?.name}"?`}
        confirmLabel="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        loading={deleteTicketType.isPending}
      />
    </div>
  );
}
