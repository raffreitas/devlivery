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

// Payment Method Colors (consistentes com o dashboard)
export const PAYMENT_METHOD_STYLES = {
  Cash: {
    bg: "bg-green-50",
    text: "text-green-700",
    border: "border-green-200",
    icon: Banknote,
  },
  CreditCard: {
    bg: "bg-blue-50",
    text: "text-blue-700",
    border: "border-blue-200",
    icon: CreditCard,
  },
  DebitCard: {
    bg: "bg-purple-50",
    text: "text-purple-700",
    border: "border-purple-200",
    icon: CreditCard,
  },
  Pix: {
    bg: "bg-teal-50",
    text: "text-teal-700",
    border: "border-teal-200",
    icon: Smartphone,
  },
} as const;

export function getPaymentMethodStyle(method: PaymentMethod) {
  return PAYMENT_METHOD_STYLES[method];
}

// Order Status Colors (consistentes com o dashboard)
export const ORDER_STATUS_STYLES = {
  pending: {
    bg: "bg-yellow-50",
    text: "text-yellow-600",
    badgeBg: "bg-yellow-100",
    badgeText: "text-yellow-800",
    icon: Clock,
    label: "Pendente",
  },
  preparing: {
    bg: "bg-blue-50",
    text: "text-blue-600",
    badgeBg: "bg-blue-100",
    badgeText: "text-blue-800",
    icon: Package,
    label: "Em Preparo",
  },
  ready: {
    bg: "bg-purple-50",
    text: "text-purple-600",
    badgeBg: "bg-purple-100",
    badgeText: "text-purple-800",
    icon: CheckCircle,
    label: "Pronto",
  },
  delivered: {
    bg: "bg-green-50",
    text: "text-green-600",
    badgeBg: "bg-green-100",
    badgeText: "text-green-800",
    icon: CheckCircle,
    label: "Entregue",
  },
  cancelled: {
    bg: "bg-red-50",
    text: "text-red-600",
    badgeBg: "bg-red-100",
    badgeText: "text-red-800",
    icon: CircleX,
    label: "Cancelado",
  },
} as const;

export function getOrderStatusStyle(status: keyof typeof ORDER_STATUS_STYLES) {
  return ORDER_STATUS_STYLES[status];
}
