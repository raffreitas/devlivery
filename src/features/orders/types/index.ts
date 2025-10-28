import type { Product } from '../../products/types';

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
  total: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface OrderFormData {
  items: OrderItem[];
  customerName: string;
  customerPhone: string;
  deliveryAddress: string;
}
