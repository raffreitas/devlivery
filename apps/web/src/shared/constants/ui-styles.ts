import {
  Banknote,
  CheckCircle,
  CircleX,
  Clock,
  CreditCard,
  Package,
  Smartphone,
} from "lucide-react";
import type { PaymentMethod } from "@/features/orders/types";

// Payment Method Colors (with dark mode support and visual distinction)
export const PAYMENT_METHOD_STYLES = {
  Cash: {
    variant: "default" as const,
    className:
      "bg-green-100 dark:bg-green-950 text-green-800 dark:text-green-200 border border-green-300 dark:border-green-700",
    icon: Banknote,
  },
  CreditCard: {
    variant: "default" as const,
    className:
      "bg-blue-100 dark:bg-blue-950 text-blue-800 dark:text-blue-200 border border-blue-300 dark:border-blue-700",
    icon: CreditCard,
  },
  DebitCard: {
    variant: "default" as const,
    className:
      "bg-indigo-100 dark:bg-indigo-950 text-indigo-800 dark:text-indigo-200 border border-indigo-300 dark:border-indigo-700",
    icon: CreditCard,
  },
  Pix: {
    variant: "default" as const,
    className:
      "bg-cyan-100 dark:bg-cyan-950 text-cyan-800 dark:text-cyan-200 border border-cyan-300 dark:border-cyan-700",
    icon: Smartphone,
  },
} as const;

export function getPaymentMethodStyle(method: PaymentMethod) {
  return PAYMENT_METHOD_STYLES[method];
}

// Order Status Colors (with dark mode support and visual distinction)
export const ORDER_STATUS_STYLES = {
  Pending: {
    variant: "default" as const,
    className:
      "bg-amber-100 dark:bg-amber-950 text-amber-800 dark:text-amber-200 border border-amber-300 dark:border-amber-700",
    icon: Clock,
    label: "Pendente",
  },
  Preparing: {
    variant: "default" as const,
    className:
      "bg-blue-100 dark:bg-blue-950 text-blue-800 dark:text-blue-200 border border-blue-300 dark:border-blue-700",
    icon: Package,
    label: "Em Preparo",
  },
  Ready: {
    variant: "default" as const,
    className:
      "bg-violet-100 dark:bg-violet-950 text-violet-800 dark:text-violet-200 border border-violet-300 dark:border-violet-700",
    icon: CheckCircle,
    label: "Pronto",
  },
  Delivered: {
    variant: "default" as const,
    className:
      "bg-green-100 dark:bg-green-950 text-green-800 dark:text-green-200 border border-green-300 dark:border-green-700",
    icon: CheckCircle,
    label: "Entregue",
  },
  Canceled: {
    variant: "default" as const,
    className:
      "bg-red-100 dark:bg-red-950 text-red-800 dark:text-red-200 border border-red-300 dark:border-red-700",
    icon: CircleX,
    label: "Cancelado",
  },
} as const;

export function getOrderStatusStyle(status: keyof typeof ORDER_STATUS_STYLES) {
  return ORDER_STATUS_STYLES[status];
}
