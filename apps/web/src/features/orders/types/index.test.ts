import { describe, expect, it } from "vitest";
import { type OrderFormData, orderFormSchema } from "./index";

const order: OrderFormData = {
  customerName: "Cliente",
  deliveryAddress: "Rua A",
  deliveryFee: 0,
  items: [
    {
      product: { id: "product", name: "Produto", price: 20 },
      unitPrice: 9.9,
      quantity: 3,
    },
  ],
  payments: [{ method: "Cash", amount: 29.7 }],
};

describe("orderFormSchema", () => {
  it("accepts an exact payment using the recorded price, independently of the catalog", () => {
    expect(orderFormSchema.safeParse(order).success).toBe(true);
  });

  it("rejects a payment one cent short on the payments field", () => {
    const result = orderFormSchema.safeParse({
      ...order,
      payments: [{ method: "Cash", amount: 29.69 }],
    });
    expect(result.success).toBe(false);
    if (!result.success)
      expect(result.error.issues[0].path).toEqual(["payments"]);
  });

  it("accepts an overpayment", () => {
    expect(
      orderFormSchema.safeParse({
        ...order,
        payments: [{ method: "Cash", amount: 30 }],
      }).success,
    ).toBe(true);
  });
});
