import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { CloseCashForm } from "./close-cash-form";

describe("CloseCashForm", () => {
  it("shows the shortage when counted cash is zero", () => {
    render(
      <CloseCashForm
        expectedCashAmount={500}
        openingAmount={500}
        depositsTotal={0}
        cashSales={0}
        onSubmit={vi.fn()}
        isSubmitting={false}
        onCancel={vi.fn()}
      />,
    );
    expect(screen.getByText("Falta no Caixa")).toBeInTheDocument();
    expect(screen.getAllByText(/500,00/).length).toBeGreaterThanOrEqual(2);
  });
});
