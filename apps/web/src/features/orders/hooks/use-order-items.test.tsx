import { act, renderHook } from "@testing-library/react";
import { useForm } from "react-hook-form";
import { describe, expect, it } from "vitest";
import type { Product } from "@/features/products/types";
import type { OrderFormData } from "../types";
import { useOrderItems } from "./use-order-items";

function product(id: string, price: number, available = true): Product {
  return {
    id,
    name: `Produto ${id}`,
    description: "",
    price,
    category: "Pizzas",
    available,
    createdAt: new Date("2026-10-17T12:00:00Z"),
    updatedAt: new Date("2026-10-17T12:00:00Z"),
  };
}

const pizza = product("pizza", 40);
const soda = product("soda", 6);
const retired = product("retired", 10, false);

function renderItems(initialItems: OrderFormData["items"] = []) {
  return renderHook(() => {
    const form = useForm<OrderFormData>({
      defaultValues: {
        customerName: "",
        deliveryAddress: "",
        deliveryFee: 0,
        payments: [{ method: "Cash", amount: 0 }],
        items: initialItems,
      },
    });
    return useOrderItems(form, [pizza, soda, retired], initialItems);
  });
}

function addItem(
  result: { current: ReturnType<typeof useOrderItems> },
  productId: string,
  quantity: number,
  notes = "",
) {
  act(() => result.current.setSelectedProductId(productId));
  act(() => result.current.setQuantity(quantity));
  act(() => result.current.setNotes(notes));
  act(() => result.current.addSelectedItem());
}

describe("useOrderItems", () => {
  it("keeps different preparation notes of the same product on separate lines", () => {
    const { result } = renderItems();

    addItem(result, pizza.id, 1, "Sem cebola");
    addItem(result, pizza.id, 1, "Com cebola");

    expect(
      result.current.fields.map((item) => [item.quantity, item.notes]),
    ).toEqual([
      [1, "Sem cebola"],
      [1, "Com cebola"],
    ]);
  });

  it("merges equivalent notes, ignoring surrounding spaces, into one line", () => {
    const { result } = renderItems();

    addItem(result, pizza.id, 1, "Sem cebola");
    addItem(result, pizza.id, 2, "  Sem cebola ");

    expect(
      result.current.fields.map((item) => [item.quantity, item.notes]),
    ).toEqual([[3, "Sem cebola"]]);
    expect(result.current.subtotal).toBe(120);
  });

  it("keeps the recorded price of products already in the order and uses the catalog price for new ones", () => {
    const { result } = renderItems([
      {
        product: { ...pizza },
        unitPrice: 35,
        quantity: 1,
        notes: undefined,
      },
    ]);

    addItem(result, pizza.id, 1, "Borda recheada");
    addItem(result, soda.id, 2);

    expect(
      result.current.fields.map((item) => [item.product.id, item.unitPrice]),
    ).toEqual([
      [pizza.id, 35],
      [pizza.id, 35],
      [soda.id, 6],
    ]);
    expect(
      result.current.selectableProducts.map((p) => [p.id, p.price]),
    ).toEqual([
      [pizza.id, 35],
      [soda.id, 6],
    ]);
  });

  it("resets the pending selection after adding an item", () => {
    const { result } = renderItems();
    act(() => result.current.setSelectedProductId(soda.id));
    expect(result.current.hasPendingSelection).toBe(true);

    act(() => result.current.addSelectedItem());

    expect(result.current.hasPendingSelection).toBe(false);
    expect(result.current.fields).toHaveLength(1);
  });
});
