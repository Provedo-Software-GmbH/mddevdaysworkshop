import { useEffect, useMemo, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { format, isPast, parseISO } from "date-fns";
import { CalendarDays, MapPin, Users, Search, ArrowRight } from "lucide-react";

import { useEvents } from "@/hooks/useEvents";
import { trackPageView } from "@/lib/telemetry";
import { cn } from "@/lib/utils";
import type { Event } from "@/types/event";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs";

function formatDateRange(startDate: string, endDate: string): string {
  const start = parseISO(startDate);
  const end = parseISO(endDate);

  const sameYear = start.getFullYear() === end.getFullYear();
  const sameMonth = sameYear && start.getMonth() === end.getMonth();
  const sameDay = sameMonth && start.getDate() === end.getDate();

  if (sameDay) return format(start, "MMM d, yyyy");
  if (sameMonth) return `${format(start, "MMM d")} - ${format(end, "d, yyyy")}`;
  if (sameYear) return `${format(start, "MMM d")} - ${format(end, "MMM d, yyyy")}`;
  return `${format(start, "MMM d, yyyy")} - ${format(end, "MMM d, yyyy")}`;
}

function EventCardSkeleton() {
  return (
    <Card className="overflow-hidden">
      <Skeleton className="h-48 w-full rounded-none" />
      <CardHeader>
        <Skeleton className="h-6 w-3/4" />
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-4 w-2/3" />
        <Skeleton className="h-4 w-1/3" />
      </CardContent>
      <CardFooter>
        <Skeleton className="h-10 w-full" />
      </CardFooter>
    </Card>
  );
}

function EventImage({ event }: { event: Event }) {
  if (event.imageUrl) {
    return (
      <img
        src={event.imageUrl}
        alt={event.title}
        className="h-48 w-full object-cover"
      />
    );
  }

  return (
    <div className="flex h-48 items-center justify-center bg-primary/10">
      <span className="text-5xl font-bold text-primary/40">
        {event.title.charAt(0).toUpperCase()}
      </span>
    </div>
  );
}

function EventCard({ event }: { event: Event }) {
  const past = isPast(parseISO(event.endDate));

  return (
    <Card className={cn("overflow-hidden transition-shadow hover:shadow-lg", past && "opacity-75")}>
      <EventImage event={event} />
      <CardHeader>
        <CardTitle className="line-clamp-2 text-lg">{event.title}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col gap-2 text-sm text-muted-foreground">
          <div className="flex items-center gap-2">
            <CalendarDays className="h-4 w-4 shrink-0" />
            <span>{formatDateRange(event.startDate, event.endDate)}</span>
          </div>
          <div className="flex items-center gap-2">
            <MapPin className="h-4 w-4 shrink-0" />
            <span className="line-clamp-1">{event.location}</span>
          </div>
          {event.maxAttendees && (
            <div className="flex items-center gap-2">
              <Users className="h-4 w-4 shrink-0" />
              <span>{event.maxAttendees} attendees max</span>
            </div>
          )}
        </div>
      </CardContent>
      <CardFooter>
        <Button asChild className="w-full">
          <Link to={`/events/${event.id}`}>
            View Details
            <ArrowRight className="ml-2 h-4 w-4" />
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}

function EmptyState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <CalendarDays className="mb-4 h-12 w-12 text-muted-foreground/50" />
      <p className="text-lg font-medium text-muted-foreground">{message}</p>
    </div>
  );
}

export default function HomePage() {
  const location = useLocation();
  const { data: events, isLoading } = useEvents();
  const [search, setSearch] = useState("");

  useEffect(() => {
    trackPageView("home", location.pathname);
  }, [location.pathname]);

  const publishedEvents = useMemo(
    () => events?.filter((e) => e.status === "Published") ?? [],
    [events],
  );

  const filteredEvents = useMemo(() => {
    const query = search.toLowerCase().trim();
    if (!query) return publishedEvents;
    return publishedEvents.filter((e) => e.title.toLowerCase().includes(query));
  }, [publishedEvents, search]);

  const upcomingEvents = useMemo(
    () => filteredEvents.filter((e) => !isPast(parseISO(e.endDate))),
    [filteredEvents],
  );

  const pastEvents = useMemo(
    () => filteredEvents.filter((e) => isPast(parseISO(e.endDate))),
    [filteredEvents],
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-12 text-center">
        <h1 className="mb-4 text-4xl font-bold tracking-tight">
          Discover Developer Events
        </h1>
        <p className="mx-auto max-w-2xl text-lg text-muted-foreground">
          Find and attend the best developer conferences, workshops, and meetups.
          Get your tickets now!
        </p>
      </div>

      <div className="relative mx-auto mb-8 max-w-md">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search events by title..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-10"
        />
      </div>

      <Tabs defaultValue="upcoming" className="w-full">
        <div className="mb-6 flex justify-center">
          <TabsList>
            <TabsTrigger value="upcoming">
              Upcoming ({isLoading ? "…" : upcomingEvents.length})
            </TabsTrigger>
            <TabsTrigger value="past">
              Past ({isLoading ? "…" : pastEvents.length})
            </TabsTrigger>
          </TabsList>
        </div>

        <TabsContent value="upcoming">
          {isLoading ? (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {Array.from({ length: 6 }, (_, i) => (
                <EventCardSkeleton key={i} />
              ))}
            </div>
          ) : upcomingEvents.length === 0 ? (
            <EmptyState message={search ? "No upcoming events match your search." : "No upcoming events at the moment."} />
          ) : (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {upcomingEvents.map((event) => (
                <EventCard key={event.id} event={event} />
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="past">
          {isLoading ? (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {Array.from({ length: 6 }, (_, i) => (
                <EventCardSkeleton key={i} />
              ))}
            </div>
          ) : pastEvents.length === 0 ? (
            <EmptyState message={search ? "No past events match your search." : "No past events to show."} />
          ) : (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {pastEvents.map((event) => (
                <EventCard key={event.id} event={event} />
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
