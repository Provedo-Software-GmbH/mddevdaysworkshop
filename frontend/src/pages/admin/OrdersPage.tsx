import { useParams, useNavigate } from "react-router-dom";
import { format, parseISO } from "date-fns";
import { ArrowLeft } from "lucide-react";

import { useOrders } from "@/hooks/useOrders";
import { useEvent } from "@/hooks/useEvents";
import type { Order, OrderStatus } from "@/types/event";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DataTable, type Column } from "@/components/shared/DataTable";
import { CurrencyDisplay } from "@/components/shared/CurrencyDisplay";
import { LoadingSpinner } from "@/components/shared/LoadingSpinner";
import { EmptyState } from "@/components/shared/EmptyState";

const statusVariant: Record<OrderStatus, "default" | "secondary" | "destructive" | "outline"> = {
  Pending: "outline",
  PaymentProcessing: "secondary",
  Paid: "default",
  Cancelled: "destructive",
  Refunded: "secondary",
};

export default function OrdersPage() {
  const { id: eventId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: event } = useEvent(eventId);
  const { data: orders, isLoading } = useOrders(eventId);

  const columns: Column<Order>[] = [
    {
      key: "orderCode",
      header: "Order Code",
      sortable: true,
      render: (o) => <span className="font-mono text-sm">{o.orderCode}</span>,
    },
    {
      key: "customerEmail",
      header: "Customer",
      sortable: true,
      render: (o) => (
        <div>
          <p className="font-medium">{o.customerName ?? "—"}</p>
          <p className="text-xs text-muted-foreground">{o.customerEmail}</p>
        </div>
      ),
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      render: (o) => <Badge variant={statusVariant[o.status]}>{o.status}</Badge>,
    },
    {
      key: "totalGross",
      header: "Total",
      sortable: true,
      render: (o) => <CurrencyDisplay amount={o.totalGross} currency={o.currency} className="font-medium" />,
    },
    {
      key: "positions",
      header: "Tickets",
      render: (o) => o.positions.length,
    },
    {
      key: "createdAt",
      header: "Date",
      sortable: true,
      render: (o) => format(parseISO(o.createdAt), "PPp"),
    },
  ];

  if (isLoading) {
    return <LoadingSpinner className="py-16" label="Loading orders…" />;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => navigate("/admin/events")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Orders</h1>
          {event && (
            <p className="text-muted-foreground">{event.title}</p>
          )}
        </div>
      </div>

      {!orders || orders.length === 0 ? (
        <EmptyState
          title="No orders yet"
          description="Orders will appear here once customers start purchasing tickets."
        />
      ) : (
        <DataTable data={orders} columns={columns} keyFn={(o) => o.id} />
      )}
    </div>
  );
}
