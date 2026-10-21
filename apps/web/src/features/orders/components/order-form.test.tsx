import { renderWithProviders } from "@test/render";
import { server } from "@test/server";
import { screen, waitFor } from "@testing-library/react";
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
  it("does not suggest cash change when an electronic amount exceeds the order", async () => {
    server.use(
      http.get("http://localhost:5000/api/v1/products", () =>
        HttpResponse.json({ data: [] }),
      ),
    );
    const onSubmit = vi.fn();
    renderWithProviders(
      <OrderForm
        initialData={{
          ...initialData,
          payments: [
            { method: "Cash", amount: 1 },
            { method: "Pix", amount: 30 },
          ],
        }}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    );
    expect(
      screen.getByText("Revise os pagamentos eletrônicos"),
    ).toBeInTheDocument();
    expect(screen.queryByText(/Troco:/)).not.toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Criar Pedido" }));
    expect(
      await screen.findByText(
        "O total dos pagamentos eletrônicos não pode ser maior que o total do pedido.",
      ),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it.each([
    ["Com cebola", 2],
    [" Sem cebola ", 1],
  ])(
    "preserves preparation instructions when adding a repeated product with %s",
    async (notes, lineCount) => {
      server.use(
        http.get("http://localhost:5000/api/v1/products", () =>
          HttpResponse.json({
            data: [
              {
                id: "product",
                name: "Pizza",
                price: 20,
                category: "Pizza",
                available: true,
                description: "Pizza",
                createdAt: "2026-10-17T12:00:00Z",
                updatedAt: "2026-10-17T12:00:00Z",
              },
            ],
          }),
        ),
      );
      const onSubmit = vi.fn().mockResolvedValue(undefined);
      const user = userEvent.setup();
      renderWithProviders(
        <OrderForm
          initialData={{
            ...initialData,
            items: [
              { ...initialData.items[0], quantity: 1, notes: "Sem cebola" },
            ],
            payments: [{ method: "CreditCard", amount: 9.9 }],
          }}
          onSubmit={onSubmit}
          onCancel={vi.fn()}
        />,
      );

      await user.type(
        screen.getByRole("combobox", { name: "Produto" }),
        "Pizza",
      );
      await user.click(await screen.findByRole("option", { name: /^Pizza -/ }));
      await user.type(
        screen.getByRole("textbox", { name: "Observações" }),
        notes,
      );
      await user.click(screen.getByRole("button", { name: "Adicionar Item" }));
      await user.click(screen.getByRole("button", { name: "Criar Pedido" }));

      await waitFor(() => expect(onSubmit).toHaveBeenCalled());
      const submitted = onSubmit.mock.calls[0][0] as OrderFormData;
      expect(submitted.items).toHaveLength(lineCount);
      expect(submitted.items[0].notes).toBe("Sem cebola");
      expect(
        submitted.items.reduce((total, item) => total + item.quantity, 0),
      ).toBe(2);
      if (lineCount === 2) expect(submitted.items[1].notes).toBe("Com cebola");
      expect(submitted.items.every((item) => item.unitPrice === 9.9)).toBe(
        true,
      );
    },
  );

  it.each([20, 5])(
    "preserves the original price when removing and readding a product whose catalog price is %s",
    async (price) => {
      server.use(
        http.get("http://localhost:5000/api/v1/products", () =>
          HttpResponse.json({
            data: [
              {
                id: "product",
                name: "Pizza",
                price,
                category: "Pizza",
                description: "Pizza",
                available: true,
                createdAt: "2026-10-17T12:00:00Z",
                updatedAt: "2026-10-17T12:00:00Z",
              },
            ],
          }),
        ),
      );
      const onSubmit = vi.fn().mockResolvedValue(undefined);
      const user = userEvent.setup();
      renderWithProviders(
        <OrderForm
          initialData={{
            ...initialData,
            id: "order",
            items: [{ ...initialData.items[0], quantity: 1 }],
            payments: [{ method: "CreditCard", amount: 9.9 }],
          }}
          onSubmit={onSubmit}
          onCancel={vi.fn()}
        />,
      );
      await user.click(
        screen.getAllByRole("button", { name: "Remover Pizza" })[0],
      );
      await user.type(
        screen.getByRole("combobox", { name: "Produto" }),
        "Pizza",
      );
      await user.click(await screen.findByRole("option", { name: /^Pizza -/ }));
      await user.click(screen.getByRole("button", { name: "Adicionar Item" }));
      expect(
        (screen.getByRole("textbox", { name: /Valor em/ }) as HTMLInputElement)
          .value,
      ).toContain("9,90");
      await user.click(
        screen.getByRole("button", { name: "Atualizar Pedido" }),
      );
      await waitFor(() =>
        expect(onSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            items: [expect.objectContaining({ unitPrice: 9.9, quantity: 1 })],
            payments: [
              expect.objectContaining({ method: "CreditCard", amount: 9.9 }),
            ],
          }),
        ),
      );
    },
  );

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
