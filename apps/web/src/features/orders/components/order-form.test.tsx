import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import type { OrderFormData } from "../types";
import { OrderForm } from "./order-form";

const initialData: OrderFormData = {
  customerName: "Cliente teste",
  deliveryAddress: "Rua A",
  deliveryFee: 0,
  items: [
    {
      product: { id: "product", name: "Pizza", price: 20 },
      unitPrice: 9.9,
      quantity: 3,
    },
  ],
  payments: [{ method: "Cash", amount: 29.69 }],
};

describe("OrderForm", () => {
  it("shows aggregate payment validation and submits an exact payment using recorded prices", async () => {
    server.use(
      http.get("http://localhost:5000/api/v1/products", () =>
        HttpResponse.json({ data: [] }),
      ),
    );
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();
    renderWithProviders(
      <OrderForm
        initialData={initialData}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Criar Pedido" }));
    expect(
      await screen.findByText(
        "O total dos pagamentos é menor que o total do pedido",
      ),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
    const payment = screen.getByRole("textbox", { name: /Valor em/ });
    await user.clear(payment);
    await user.type(payment, "2970");
    await user.click(screen.getByRole("button", { name: "Criar Pedido" }));
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        items: [expect.objectContaining({ unitPrice: 9.9 })],
      }),
    );
  });
});
