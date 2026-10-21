import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { Order } from "../types";
import { OrderPrint } from "./order-print";

function makeOrder(id: string): Order {
  return {
    id,
    customerName: "Cliente",
    deliveryAddress: "Rua A",
    status: "Pending",
    items: [
      {
        product: {
          id: "pizza",
          name: "Pizza",
          price: 10,
          category: "Pizza",
          description: "Pizza",
          available: true,
          createdAt: new Date(),
          updatedAt: new Date(),
        },
        unitPrice: 10,
        quantity: 1,
      },
    ],
    payments: [
      { id: "cash", method: "Cash", amount: 10 },
      { id: "pix", method: "Pix", amount: 6 },
    ],
    total: 10,
    deliveryFee: 0,
    createdAt: new Date(),
    updatedAt: new Date(),
  };
}

describe("OrderPrint", () => {
  it("never prints a cash-change instruction for a legacy electronic overpayment", () => {
    render(
      <OrderPrint
        order={{
          ...makeOrder("legacy"),
          payments: [
            { id: "cash", method: "Cash", amount: 1 },
            { id: "pix", method: "Pix", amount: 20 },
          ],
        }}
      />,
    );
    expect(screen.queryByText("TROCO:")).not.toBeInTheDocument();
  });

  it("prints every mixed-payment amount and the cash change needed before delivery", () => {
    render(<OrderPrint order={makeOrder("order")} />);
    expect(screen.getByText("- Pix").parentElement).toHaveTextContent(/6,00/);
    expect(screen.getByText("- Dinheiro").parentElement).toHaveTextContent(
      /10,00/,
    );
    expect(screen.getByText("TROCO:").parentElement).toHaveTextContent(/6,00/);
  });

  it("prints only the last eight UUID characters in uppercase for orders sharing a prefix", () => {
    const first = "01999999-0000-7000-8000-0000abcdef01";
    const second = "01999999-0000-7000-8000-0000abcdef02";
    render(
      <>
        <OrderPrint order={makeOrder(first)} />
        <OrderPrint order={makeOrder(second)} />
      </>,
    );
    expect(screen.getByText("Pedido #ABCDEF01")).toBeInTheDocument();
    expect(screen.getByText("Pedido #ABCDEF02")).toBeInTheDocument();
    expect(
      screen.queryByText(first.toUpperCase(), { exact: false }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(second.toUpperCase(), { exact: false }),
    ).not.toBeInTheDocument();
  });

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
