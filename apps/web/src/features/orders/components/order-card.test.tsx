import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import type { Order } from "../types";
import { OrderCard } from "./order-card";

const order: Order = {
  id: "order",
  customerName: "Cliente",
  deliveryAddress: "Rua A",
  status: "Ready",
  items: [],
  payments: [{ id: "cash", method: "Cash", amount: 10 }],
  total: 10,
  deliveryFee: 0,
  createdAt: new Date(),
  updatedAt: new Date(),
};

describe("OrderCard financial protections", () => {
  it("allows delivery without depending on a cash session", async () => {
    const onUpdateStatus = vi.fn();
    render(
      <OrderCard
        order={order}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
        onUpdateStatus={onUpdateStatus}
      />,
    );
    const deliver = screen.getByRole("button", {
      name: /Marcar como Entregue/,
    });
    expect(deliver).toBeEnabled();
    await userEvent.click(deliver);
    expect(onUpdateStatus).toHaveBeenCalledWith(order.id, "Delivered");
  });

  it("prevents another delivery request while the first is in flight", async () => {
    let finish: () => void = () => {};
    const onUpdateStatus = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          finish = resolve;
        }),
    );
    render(
      <OrderCard
        order={order}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
        onUpdateStatus={onUpdateStatus}
      />,
    );
    const user = userEvent.setup();
    const deliver = screen.getByRole("button", {
      name: /Marcar como Entregue/,
    });
    await user.click(deliver);
    expect(deliver).toBeDisabled();
    await user.click(deliver);
    expect(onUpdateStatus).toHaveBeenCalledTimes(1);
    finish();
    await waitFor(() => expect(deliver).toBeEnabled());
  });

  it("keeps printing available but never offers deletion of a delivered sale", () => {
    render(
      <OrderCard
        order={{ ...order, status: "Delivered" }}
        onEdit={vi.fn()}
        onDelete={vi.fn()}
        onUpdateStatus={vi.fn()}
      />,
    );
    expect(
      screen.queryByRole("button", { name: "Excluir" }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Imprimir" })).toBeEnabled();
  });
});
