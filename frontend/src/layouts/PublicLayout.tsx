import { Link, Outlet, useLocation } from "react-router-dom";
import { Calendar, Ticket } from "lucide-react";
import { useEffect } from "react";
import { trackPageView } from "@/lib/telemetry";
import { cn } from "@/lib/utils";

const navItems = [
  { label: "Events", href: "/" },
  { label: "About", href: "/about" },
];

export default function PublicLayout() {
  const location = useLocation();

  useEffect(() => {
    trackPageView("public", location.pathname);
  }, [location.pathname]);

  return (
    <div className="flex min-h-screen flex-col">
      <header className="sticky top-0 z-50 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container mx-auto flex h-16 items-center justify-between px-4">
          <Link to="/" className="flex items-center gap-2 font-bold text-xl">
            <Ticket className="h-6 w-6 text-primary" />
            <span>DevConf Ticketing</span>
          </Link>

          <nav className="flex items-center gap-6">
            {navItems.map((item) => (
              <Link
                key={item.href}
                to={item.href}
                className={cn(
                  "text-sm font-medium transition-colors hover:text-primary",
                  location.pathname === item.href
                    ? "text-foreground"
                    : "text-muted-foreground",
                )}
              >
                {item.label}
              </Link>
            ))}
            <Link
              to="/admin"
              className="text-sm font-medium text-muted-foreground transition-colors hover:text-primary"
            >
              Admin
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t bg-muted/50">
        <div className="container mx-auto flex flex-col items-center gap-4 px-4 py-8 md:flex-row md:justify-between">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Calendar className="h-4 w-4" />
            <span>DevConf Ticketing &mdash; MD DevDays 2026</span>
          </div>
          <p className="text-sm text-muted-foreground">
            Built with React, .NET &amp; GitHub Copilot
          </p>
        </div>
      </footer>
    </div>
  );
}
