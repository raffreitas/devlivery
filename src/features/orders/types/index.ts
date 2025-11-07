import type { Product } from "@/features/products/types";

export type PaymentMethod = "Cash" | "CreditCard" | "DebitCard" | "Pix";

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
