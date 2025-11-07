import type { Product } from "@/features/products/types";

export const PAYMENT_METHODS = {
  Cash: "Dinheiro",
  Card: "Cartão",
  Pix: "Pix",
} as const;

export type PaymentMethod = keyof typeof PAYMENT_METHODS;

export interface OrderItem {
  product: Product;
  quantity: number;
  notes?: string;
}

export interface Order {
  id: string;
  items: OrderItem[];
  customerName: string;
  customerPhone: string;
  deliveryAddress: string;
  status: "pending" | "preparing" | "ready" | "delivered" | "cancelled";
  paymentMethod: PaymentMethod;
  total: number;
  deliveryFee: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface OrderFormData {
  items: OrderItem[];
  customerName: string;
  customerPhone?: string;
  paymentMethod: PaymentMethod;
  deliveryAddress: string;
  deliveryFee: number;
}
