import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider, ProtectedRoute } from "@/lib/auth";
import { ErrorBoundary } from "@/components/shared/ErrorBoundary";
import PublicLayout from "@/layouts/PublicLayout";
import AdminLayout from "@/layouts/AdminLayout";
import HomePage from "@/pages/public/HomePage";
import EventDetailPage from "@/pages/public/EventDetailPage";
import TicketSelectionPage from "@/pages/public/TicketSelectionPage";
import CheckoutPage from "@/pages/public/CheckoutPage";
import AdminDashboardPage from "@/pages/admin/AdminDashboardPage";
import EventsPage from "@/pages/admin/EventsPage";
import CreateEventPage from "@/pages/admin/CreateEventPage";
import EditEventPage from "@/pages/admin/EditEventPage";
import TicketTypesPage from "@/pages/admin/TicketTypesPage";
import TaxRatesPage from "@/pages/admin/TaxRatesPage";
import OrdersPage from "@/pages/admin/OrdersPage";
import NotFoundPage from "@/pages/NotFoundPage";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <TooltipProvider>
          <ErrorBoundary>
            <BrowserRouter>
              <Routes>
                {/* Public routes */}
                <Route element={<PublicLayout />}>
                  <Route index element={<HomePage />} />
                  <Route path="events/:id" element={<EventDetailPage />} />
                  <Route path="events/:id/tickets" element={<TicketSelectionPage />} />
                  <Route path="checkout" element={<CheckoutPage />} />
                </Route>

                {/* Admin routes — protected by auth */}
                <Route
                  path="admin"
                  element={
                    <ProtectedRoute>
                      <AdminLayout />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<AdminDashboardPage />} />
                  <Route path="events" element={<EventsPage />} />
                  <Route path="events/new" element={<CreateEventPage />} />
                  <Route path="events/:id/edit" element={<EditEventPage />} />
                  <Route path="events/:id/ticket-types" element={<TicketTypesPage />} />
                  <Route path="events/:id/orders" element={<OrdersPage />} />
                  <Route path="tax-rates" element={<TaxRatesPage />} />
                </Route>

                {/* Catch-all */}
                <Route path="*" element={<NotFoundPage />} />
              </Routes>
            </BrowserRouter>
          </ErrorBoundary>
          <Toaster />
        </TooltipProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}
