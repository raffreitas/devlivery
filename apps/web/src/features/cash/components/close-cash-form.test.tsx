import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { CloseCashForm } from "./close-cash-form";

function renderForm(onSubmit = vi.fn(async () => {})) {
  render(
    <CloseCashForm
      expectedCashAmount={500}
      openingAmount={100}
      depositsTotal={50}
      cashPayments={350}
      cashRefunds={0}
      changeAmount={0}
      onSubmit={onSubmit}
      isSubmitting={false}
    />,
  );
  return onSubmit;
}

describe("CloseCashForm", () => {
  it("reveals the shortage only after confirming a zero count without closing the session", async () => {
    const user = userEvent.setup();
    const submit = renderForm();
    expect(screen.queryByText("Esperado em dinheiro")).not.toBeInTheDocument();
    expect(screen.queryByText(/500,00/)).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Conferir contagem" }));
    expect(await screen.findByText("Falta no caixa")).toBeVisible();
    expect(screen.getByText(/Diferença:.*500,00/)).toBeVisible();
    expect(submit).not.toHaveBeenCalled();
  });

  it("allows correction before closing and submits the final count once", async () => {
    const user = userEvent.setup();
    const submit = renderForm();
    const amount = screen.getByRole("textbox", { name: "Dinheiro contado" });
    fireEvent.change(amount, { target: { value: "49000" } });
    await user.click(screen.getByRole("button", { name: "Conferir contagem" }));
    await user.click(screen.getByRole("button", { name: "Contar de novo" }));
    expect(screen.queryByText("Esperado em dinheiro")).not.toBeInTheDocument();
    fireEvent.change(
      screen.getByRole("textbox", { name: "Dinheiro contado" }),
      { target: { value: "50000" } },
    );
    await user.click(screen.getByRole("button", { name: "Conferir contagem" }));
    expect(await screen.findByText("Caixa conferido")).toBeVisible();
    await user.type(
      screen.getByRole("textbox", {
        name: "Observação do fechamento (opcional)",
      }),
      " Conferido ",
    );
    await user.click(screen.getByRole("button", { name: "Fechar caixa" }));
    expect(submit).toHaveBeenCalledExactlyOnceWith({
      closingAmount: 500,
      notes: "Conferido",
    });
  });

  it("preserves counted cash and allows retry after a rejected close", async () => {
    const user = userEvent.setup();
    renderForm(
      vi.fn(async () => {
        throw new Error("Falha no fechamento");
      }),
    );
    fireEvent.change(
      screen.getByRole("textbox", { name: "Dinheiro contado" }),
      { target: { value: "50000" } },
    );
    await user.click(screen.getByRole("button", { name: "Conferir contagem" }));
    await user.click(screen.getByRole("button", { name: "Fechar caixa" }));
    expect(await screen.findByRole("alert")).toBeInTheDocument();
    expect(screen.getByText("Contado").parentElement).toHaveTextContent(
      "500,00",
    );
    expect(screen.getByRole("button", { name: "Fechar caixa" })).toBeEnabled();
  });
});
