import { useEffect } from "react";
import { Link, useParams, useLocation } from "react-router-dom";
import { format, parseISO } from "date-fns";
import {
  CalendarDays,
  MapPin,
  Users,
  Globe,
  Ticket,
  ArrowLeft,
  Clock,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Separator } from "@/components/ui/separator";
import { useEvent } from "@/hooks/useEvents";
import { useTicketTypes } from "@/hooks/useTicketTypes";
import { trackPageView } from "@/lib/telemetry";
import { cn } from "@/lib/utils";
import type { TicketType } from "@/types/event";

function formatPrice(price: number, currency: string): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency: currency || "EUR",
  }).format(price);
}

function statusVariant(
  status: string
): "default" | "secondary" | "destructive" | "outline" {
  switch (status) {
    case "Published":
      return "default";
    case "Cancelled":
      return "destructive";
    case "Draft":
      return "secondary";
    default:
      return "outline";
  }
}

function LoadingSkeleton() {
  return (
    <div className="container mx-auto px-4 py-8 space-y-8">
      <Skeleton className="h-6 w-32" />
      <Skeleton className="h-64 w-full rounded-xl" />
      <div className="space-y-4">
        <Skeleton className="h-10 w-3/4" />
        <Skeleton className="h-5 w-1/4" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
        <Skeleton className="h-32 w-full" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-48 w-full" />
          ))}
        </div>
      </div>
    </div>
  );
}

function NotFound() {
  return (
    <div className="container mx-auto px-4 py-16 text-center">
      <h1 className="text-4xl font-bold mb-4">Event Not Found</h1>
      <p className="text-muted-foreground mb-8">
        The event you're looking for doesn't exist or has been removed.
      </p>
      <Button asChild>
        <Link to="/events">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to Events
        </Link>
      </Button>
    </div>
  );
}

function TicketTypeCard({ ticket }: { ticket: TicketType }) {
  const remaining = ticket.availableQuantity - ticket.soldQuantity;
  const soldOut = remaining <= 0;

  return (
    <Card className={cn(soldOut && "opacity-60")}>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span className="flex items-center gap-2">
            <Ticket className="h-5 w-5" />
            {ticket.name}
          </span>
          <span className="text-xl font-bold">
            {formatPrice(ticket.price, ticket.currency)}
          </span>
        </CardTitle>
        {ticket.description && (
          <CardDescription>{ticket.description}</CardDescription>
        )}
      </CardHeader>
      <CardContent>
        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          {ticket.showRemainingQuantity && (
            <span>
              {soldOut
                ? "Sold out"
                : `${remaining} remaining`}
            </span>
          )}
          {ticket.maxPerOrder > 0 && (
            <span>Max {ticket.maxPerOrder} per order</span>
          )}
        </div>
      </CardContent>
      <CardFooter>
        {soldOut ? (
          <Badge variant="destructive">Sold Out</Badge>
        ) : (
          <Badge variant="secondary">Available</Badge>
        )}
      </CardFooter>
    </Card>
  );
}

export default function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const { data: event, isLoading, isError } = useEvent(id);
  const { data: ticketTypes, isLoading: ticketsLoading } = useTicketTypes(id);

  useEffect(() => {
    trackPageView("eventDetail", location.pathname);
  }, [location.pathname]);

  if (isLoading) return <LoadingSkeleton />;
  if (isError || !event) return <NotFound />;

  return (
    <div className="container mx-auto px-4 py-8 space-y-8">
      {/* Back link */}
      <Link
        to="/events"
        className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Events
      </Link>

      {/* Hero section */}
      <div className="relative overflow-hidden rounded-xl">
        {event.imageUrl ? (
          <img
            src={event.imageUrl}
            alt={event.title}
            className="w-full h-64 sm:h-80 object-cover"
          />
        ) : (
          <div className="w-full h-64 sm:h-80 bg-gradient-to-br from-primary/20 via-primary/10 to-background flex items-center justify-center">
            <CalendarDays className="h-20 w-20 text-primary/30" />
          </div>
        )}
      </div>

      {/* Title and status */}
      <div className="space-y-2">
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-3xl sm:text-4xl font-bold tracking-tight">
            {event.title}
          </h1>
          <Badge variant={statusVariant(event.status)}>{event.status}</Badge>
        </div>
      </div>

      {/* Info grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <CalendarDays className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-sm">
              <p className="font-medium">Start</p>
              <p className="text-muted-foreground">
                {format(parseISO(event.startDate), "PPP 'at' p")}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <Clock className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-sm">
              <p className="font-medium">End</p>
              <p className="text-muted-foreground">
                {format(parseISO(event.endDate), "PPP 'at' p")}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <MapPin className="h-5 w-5 text-muted-foreground shrink-0" />
            <div className="text-sm">
              <p className="font-medium">Location</p>
              <p className="text-muted-foreground">{event.location}</p>
            </div>
          </CardContent>
        </Card>

        {event.maxAttendees && (
          <Card>
            <CardContent className="flex items-center gap-3 pt-6">
              <Users className="h-5 w-5 text-muted-foreground shrink-0" />
              <div className="text-sm">
                <p className="font-medium">Max Attendees</p>
                <p className="text-muted-foreground">
                  {event.maxAttendees.toLocaleString("de-DE")}
                </p>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Website link */}
      {event.websiteUrl && (
        <a
          href={event.websiteUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 text-sm text-primary hover:underline"
        >
          <Globe className="h-4 w-4" />
          Event Website
        </a>
      )}

      <Separator />

      {/* Description */}
      <section className="space-y-3">
        <h2 className="text-2xl font-semibold">About this Event</h2>
        <p className="text-muted-foreground whitespace-pre-line leading-relaxed">
          {event.description}
        </p>
      </section>

      <Separator />

      {/* Ticket types */}
      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">Tickets</h2>
        {ticketsLoading ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-48 w-full rounded-xl" />
            ))}
          </div>
        ) : ticketTypes && ticketTypes.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {ticketTypes.map((ticket) => (
              <TicketTypeCard key={ticket.id} ticket={ticket} />
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">
            No ticket types available yet.
          </p>
        )}
      </section>

      {/* Buy tickets CTA */}
      {ticketTypes && ticketTypes.length > 0 && (
        <div className="flex justify-center pt-4 pb-8">
          <Button asChild size="lg" className="text-lg px-8">
            <Link to={`/events/${id}/tickets`}>
              <Ticket className="mr-2 h-5 w-5" />
              Buy Tickets
            </Link>
          </Button>
        </div>
      )}
    </div>
  );
}
