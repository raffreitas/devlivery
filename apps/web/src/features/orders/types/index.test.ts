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
  it.each(["Pix", "CreditCard", "DebitCard"])(
    "rejects excess in %s even with a cash payment",
    (method) => {
      const result = orderFormSchema.safeParse({
        ...order,
        payments: [
          { method: "Cash", amount: 1 },
          { method, amount: 30 },
        ],
      });
      expect(result.success).toBe(false);
      if (!result.success)
        expect(result.error.issues).toContainEqual(
          expect.objectContaining({
            path: ["payments"],
            message:
              "O total dos pagamentos eletrônicos não pode ser maior que o total do pedido.",
          }),
        );
    },
  );

  it("validates the sum of electronic methods and permits cash change only on the remainder", () => {
    expect(
      orderFormSchema.safeParse({
        ...order,
        payments: [
          { method: "Pix", amount: 20 },
          { method: "CreditCard", amount: 10 },
        ],
      }).success,
    ).toBe(false);
    expect(
      orderFormSchema.safeParse({
        ...order,
        payments: [
          { method: "Pix", amount: 20 },
          { method: "Cash", amount: 10 },
        ],
      }).success,
    ).toBe(true);
  });

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
