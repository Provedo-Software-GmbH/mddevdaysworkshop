import { Link, Outlet, useLocation } from "react-router-dom";
import { useEffect } from "react";
import {
  CalendarDays,
  LayoutDashboard,
  Receipt,
  Settings,
  Ticket,
  Tags,
} from "lucide-react";
import { trackPageView } from "@/lib/telemetry";
import { cn } from "@/lib/utils";
import { Separator } from "@/components/ui/separator";
import { getCurrentUser } from "@/lib/auth";

const sidebarItems = [
  { label: "Dashboard", href: "/admin", icon: LayoutDashboard },
  { label: "Events", href: "/admin/events", icon: CalendarDays },
  { label: "Tax Rates", href: "/admin/tax-rates", icon: Receipt },
  { label: "Orders", href: "/admin/orders", icon: Tags },
  { label: "Settings", href: "/admin/settings", icon: Settings },
];

export default function AdminLayout() {
  const location = useLocation();
  const user = getCurrentUser();

  useEffect(() => {
    trackPageView("admin", location.pathname);
  }, [location.pathname]);

  return (
    <div className="flex min-h-screen">
      {/* Sidebar */}
      <aside className="sticky top-0 flex h-screen w-64 flex-col border-r bg-muted/30">
        <div className="flex h-16 items-center gap-2 border-b px-6">
          <Ticket className="h-5 w-5 text-primary" />
          <span className="font-semibold">Admin Panel</span>
        </div>

        <nav className="flex-1 space-y-1 p-4">
          {sidebarItems.map((item) => {
            const isActive =
              location.pathname === item.href ||
              (item.href !== "/admin" &&
                location.pathname.startsWith(item.href));

            return (
              <Link
                key={item.href}
                to={item.href}
                className={cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                )}
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <Separator />

        <div className="p-4">
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-xs font-medium text-primary-foreground">
              {user?.name?.charAt(0) ?? "A"}
            </div>
            <div className="flex flex-col">
              <span className="text-sm font-medium">{user?.name ?? "Admin"}</span>
              <span className="text-xs text-muted-foreground">
                {user?.email ?? "admin@devconf.local"}
              </span>
            </div>
          </div>
        </div>

        <div className="border-t p-4">
          <Link
            to="/"
            className="text-sm text-muted-foreground transition-colors hover:text-foreground"
          >
            ← Back to public site
          </Link>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex flex-1 flex-col">
        <header className="flex h-16 items-center border-b px-6">
          <h2 className="text-lg font-semibold">
            {sidebarItems.find(
              (item) =>
                location.pathname === item.href ||
                (item.href !== "/admin" &&
                  location.pathname.startsWith(item.href)),
            )?.label ?? "Admin"}
          </h2>
        </header>

        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
