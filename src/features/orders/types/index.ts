import z from "zod";
import type { Product } from "@/features/products/types";
import { isValidBrazilianPhone } from "@/shared/utils/validators";

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
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

export const orderFormSchema = z.object({
  customerName: z
    .string({ error: "Nome do cliente é obrigatório" })
    .min(3, "Nome do cliente deve ter pelo menos 3 caracteres")
    .max(200, "Nome do cliente deve ter no máximo 200 caracteres")
    .trim(),
  customerPhone: z
    .string()
    .max(20, "Telefone deve ter no máximo 20 caracteres")
    .refine(isValidBrazilianPhone, {
      message: "Número de telefone deve ter entre 10 e 11 dígitos",
    })
    .optional()
    .or(z.literal("")),
  deliveryAddress: z
    .string({ error: "Endereço de entrega é obrigatório" })
    .min(1, "Endereço de entrega é obrigatório")
    .max(500, "Endereço de entrega deve ter no máximo 500 caracteres")
    .trim(),
  deliveryFee: z
    .number({ error: "Taxa de entrega deve ser um número" })
    .min(0, "Taxa de entrega deve ser maior ou igual a 0"),
  paymentMethod: z.enum(["CreditCard", "DebitCard", "Cash", "Pix"]),
  notes: z
    .string()
    .max(500, "Observações devem ter no máximo 500 caracteres")
    .optional()
    .or(z.literal("")),
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
