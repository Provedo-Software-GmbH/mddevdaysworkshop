export type EventStatus = "Draft" | "Published" | "Cancelled" | "Archived";

export interface Event {
  id: string;
  title: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string;
  organizerId: string;
  status: EventStatus;
  imageUrl: string | null;
  websiteUrl: string | null;
  maxAttendees: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface LineItemTemplate {
  name: string;
  netAmount: number;
  taxRateId: string;
  taxRateName: string;
  taxRatePercentage: number;
}

export interface TicketType {
  id: string;
  eventId: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  availableQuantity: number;
  soldQuantity: number;
  maxPerOrder: number;
  showRemainingQuantity: boolean;
  saleStart: string | null;
  saleEnd: string | null;
  lineItems: LineItemTemplate[];
}
