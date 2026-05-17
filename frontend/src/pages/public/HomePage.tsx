import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { CalendarDays, MapPin, Users } from "lucide-react";
import { Link } from "react-router-dom";

export default function HomePage() {
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

      {/* Placeholder for event listing — will be implemented in Step 7 */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {[1, 2, 3].map((i) => (
          <Card key={i} className="overflow-hidden">
            <div className="h-48 bg-muted" />
            <CardHeader>
              <CardTitle>Event Placeholder {i}</CardTitle>
              <CardDescription>
                This will be replaced with real event data
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="flex flex-col gap-2 text-sm text-muted-foreground">
                <div className="flex items-center gap-2">
                  <CalendarDays className="h-4 w-4" />
                  <span>Coming soon</span>
                </div>
                <div className="flex items-center gap-2">
                  <MapPin className="h-4 w-4" />
                  <span>TBD</span>
                </div>
                <div className="flex items-center gap-2">
                  <Users className="h-4 w-4" />
                  <span>Capacity TBD</span>
                </div>
              </div>
              <Button asChild className="mt-4 w-full">
                <Link to={`/events/placeholder-${i}`}>View Details</Link>
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
