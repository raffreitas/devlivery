import type { PaymentMethod } from "../types";

export const PAYMENT_METHODS: Record<PaymentMethod, string> = {
  Cash: "Dinheiro",
  CreditCard: "Cart. Crédito",
  DebitCard: "Cart. Débito",
  Pix: "Pix",
} as const;

export function getPaymentOptions() {
  return (Object.keys(PAYMENT_METHODS) as PaymentMethod[]).map((key) => ({
    value: key,
    label: PAYMENT_METHODS[key],
  }));
}

export function getPaymentOptionLabel(
  value?: keyof typeof PAYMENT_METHODS | string | null,
) {
  if (!value) return "-";
  if (value in PAYMENT_METHODS) {
    return PAYMENT_METHODS[value as keyof typeof PAYMENT_METHODS];
  }
  return String(value);
}
