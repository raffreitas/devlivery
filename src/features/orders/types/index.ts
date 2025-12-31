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
export interface OrderPayment {
  id: string;
  method: PaymentMethod;
  amount: number;
}

export interface Order {
  id: string;
  items: OrderItem[];
  customerName: string;
  customerPhone?: string;
  deliveryAddress: string;
  status: OrderStatus;
  payments: OrderPayment[];
  total: number;
  deliveryFee: number;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

export const orderFormSchema = z
  .object({
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
    payments: z
      .array(
        z.object({
          id: z.string().optional(),
          method: z.enum(["CreditCard", "DebitCard", "Cash", "Pix"]),
          amount: z.number().min(0.01, "O valor deve ser maior que zero"),
        }),
      )
      .min(1, "Adicione pelo menos uma forma de pagamento"),
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
  })
  .superRefine((data, ctx) => {
    const subtotal = data.items.reduce(
      (sum, item) => sum + item.product.price * item.quantity,
      0,
    );
    const totalOrder = subtotal + data.deliveryFee;
    const totalPayments = data.payments.reduce((sum, p) => sum + p.amount, 0);

    // Usamos uma pequena tolerância para evitar problemas de precisão de ponto flutuante
    if (Math.abs(totalPayments - totalOrder) > 0.01) {
      ctx.addIssue({
        code: "custom",
        message: `A soma dos pagamentos (R$ ${totalPayments.toFixed(2)}) deve ser igual ao total do pedido (R$ ${totalOrder.toFixed(2)})`,
        path: ["payments"],
      });
    }
  });

export type OrderFormData = z.infer<typeof orderFormSchema>;
