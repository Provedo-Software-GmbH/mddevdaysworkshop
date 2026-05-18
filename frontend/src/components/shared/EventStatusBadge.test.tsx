import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { EventStatusBadge } from "@/components/shared/EventStatusBadge";

describe("EventStatusBadge", () => {
  it("renders Draft status with correct label", () => {
    render(<EventStatusBadge status="Draft" />);
    expect(screen.getByText("Draft")).toBeInTheDocument();
  });

  it("renders Published status with correct label", () => {
    render(<EventStatusBadge status="Published" />);
    expect(screen.getByText("Published")).toBeInTheDocument();
  });

  it("renders Cancelled status with correct label", () => {
    render(<EventStatusBadge status="Cancelled" />);
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("renders Archived status with correct label", () => {
    render(<EventStatusBadge status="Archived" />);
    expect(screen.getByText("Archived")).toBeInTheDocument();
  });
});
