import type { OrderStatus } from "../types";

export const ORDER_STATUS: Record<OrderStatus, string> = {
  Pending: "Pendente",
  Preparing: "Em Preparo",
  Ready: "Pronto",
  Delivered: "Entregue",
  Canceled: "Cancelado",
} as const;

export function getOrderStatusOptions() {
  return (Object.keys(ORDER_STATUS) as OrderStatus[]).map((key) => ({
    value: key,
    label: ORDER_STATUS[key],
  }));
}

export function getOrderStatusOptionLabel(
  value?: keyof typeof ORDER_STATUS | string | null,
) {
  if (!value) return "-";
  if (value in ORDER_STATUS) {
    return ORDER_STATUS[value as keyof typeof ORDER_STATUS];
  }
  return String(value);
}
