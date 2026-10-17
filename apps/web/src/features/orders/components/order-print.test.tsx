import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { Order } from "../types";
import { OrderPrint } from "./order-print";

describe("OrderPrint", () => {
  it("prints historical prices even after catalog prices change", () => {
    const order: Order = {
      id: "order",
      customerName: "Cliente",
      deliveryAddress: "Rua A",
      status: "Pending",
      items: [
        {
          product: {
            id: "product",
            name: "Pizza",
            price: 20,
            category: "Pizza",
            description: "Pizza",
            available: true,
            createdAt: new Date(),
            updatedAt: new Date(),
          },
          unitPrice: 9.9,
          quantity: 3,
        },
      ],
      payments: [],
      total: 29.7,
      deliveryFee: 0,
      createdAt: new Date(),
      updatedAt: new Date(),
    };
    render(<OrderPrint order={order} />);
    expect(screen.getAllByText(/29,70/)).toHaveLength(3);
    expect(screen.queryByText(/60,00/)).not.toBeInTheDocument();
  });
});
