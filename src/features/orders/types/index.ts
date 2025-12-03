import z from "zod";
import type { Product } from "@/features/products/types";

export type PaymentMethod = "Cash" | "CreditCard" | "DebitCard" | "Pix";
export type OrderStatus =
  | "Pending"
  | "Preparing"
  | "Ready"
  | "Delivered"
  | "Canceled";

export interface OrderItem {
  product: Product;
  quantity: number;
  notes?: string;
}

export interface Order {
  id: string;
  items: OrderItem[];
  customerName: string;
  customerPhone?: string;
  deliveryAddress: string;
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  total: number;
  deliveryFee: number;
  createdAt: Date;
  updatedAt: Date;
}

export const orderFormSchema = z.object({
  customerName: z.string().min(1, "Nome do cliente é obrigatório"),
  customerPhone: z.string().optional(),
  deliveryAddress: z.string().min(1, "Endereço de entrega é obrigatório"),
  deliveryFee: z.number().min(0, "Taxa de entrega deve ser maior ou igual a 0"),
  paymentMethod: z.enum(["CreditCard", "DebitCard", "Cash", "Pix"]),
  items: z
    .array(
      z.object({
        product: z.object({
          id: z.string(),
          name: z.string(),
          price: z.number(),
          description: z.string().optional(),
          category: z.string().optional(),
          available: z.boolean().optional(),
        }),
        quantity: z.number().min(1, "Quantidade deve ser maior que 0"),
        notes: z.string().optional(),
      }),
    )
    .min(1, "Adicione pelo menos um produto ao pedido"),
});

export type OrderFormData = z.infer<typeof orderFormSchema>;
