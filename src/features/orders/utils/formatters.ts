import { PAYMENT_METHODS } from "../types";

export function formatPaymentMethod(
  value?: keyof typeof PAYMENT_METHODS | string | null,
) {
  if (!value) return "-";
  // If it's one of the known methods, return the localized label.
  if (value in PAYMENT_METHODS) {
    return PAYMENT_METHODS[value as keyof typeof PAYMENT_METHODS];
  }
  // Fallback to the raw value
  return String(value);
}
