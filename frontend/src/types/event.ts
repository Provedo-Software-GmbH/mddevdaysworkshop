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

export interface TaxRate {
  id: string;
  countryCode: string;
  name: string;
  percentage: number;
  description: string;
  isDefault: boolean;
  isActive: boolean;
}

export type OrderStatus = "Pending" | "PaymentProcessing" | "Paid" | "Cancelled" | "Refunded";

export interface OrderPosition {
  ticketTypeId: string;
  ticketTypeName: string;
  attendeeName: string | null;
  attendeeEmail: string | null;
  ticketSecret: string;
  checkedIn: boolean;
  checkedInAt: string | null;
}

export interface Order {
  id: string;
  orderCode: string;
  eventId: string;
  customerEmail: string;
  customerName: string | null;
  customerId: string | null;
  voucherCode: string | null;
  status: OrderStatus;
  positions: OrderPosition[];
  totalNet: number;
  totalTax: number;
  totalGross: number;
  discountAmount: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
  cancellationDate: string | null;
}
