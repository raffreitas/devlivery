import { act, renderHook } from "@testing-library/react";
import { useForm } from "react-hook-form";
import { describe, expect, it } from "vitest";
import type { OrderFormData } from "../types";
import { useOrderPayments } from "./use-order-payments";

function renderPayments(
  payments: OrderFormData["payments"],
  subtotal: number,
  deliveryFee = 0,
) {
  return renderHook(() => {
    const form = useForm<OrderFormData>({
      defaultValues: {
        customerName: "",
        deliveryAddress: "",
        deliveryFee,
        payments,
        items: [],
      },
    });
    return { form, ...useOrderPayments(form, subtotal) };
  });
}

describe("useOrderPayments", () => {
  it("flags electronic payments above the order total instead of suggesting change", () => {
    const { result } = renderPayments(
      [
        { method: "Pix", amount: 30 },
        { method: "Cash", amount: 20 },
      ],
      45,
      5,
    );

    expect(result.current.total).toBe(50);
    expect(result.current.hasElectronicExcess).toBe(false);

    act(() => result.current.form.setValue("payments.0.amount", 50.01));

    expect(result.current.hasElectronicExcess).toBe(true);
  });

  it("reports change when cash exceeds the total and the missing amount otherwise", () => {
    const { result } = renderPayments([{ method: "Cash", amount: 100 }], 70.1);

    expect(result.current.remainingAmount).toBeCloseTo(-29.9);
    expect(result.current.hasElectronicExcess).toBe(false);

    act(() => result.current.form.setValue("payments.0.amount", 50));

    expect(result.current.remainingAmount).toBeCloseTo(20.1);
  });

  it("keeps a single non-cash payment equal to the order total", () => {
    const { result } = renderPayments([{ method: "Pix", amount: 0 }], 42, 8);

    expect(result.current.form.getValues("payments.0.amount")).toBe(50);
  });

  it("adds a method with the remaining amount and never removes the last one", () => {
    const { result } = renderPayments([{ method: "Cash", amount: 30 }], 50);

    act(() => result.current.changePaymentMethods(["Cash", "Pix"]));
    expect(result.current.form.getValues("payments")).toEqual([
      { method: "Cash", amount: 30 },
      { method: "Pix", amount: 20 },
    ]);

    act(() => result.current.changePaymentMethods(["Pix"]));
    act(() => result.current.changePaymentMethods([]));
    expect(result.current.paymentFields.map((p) => p.method)).toEqual(["Pix"]);
  });
});
