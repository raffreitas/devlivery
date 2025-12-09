import z from "zod";

export type CashSessionStatus = "open" | "closed";

export interface PaymentMethodTotal {
  method: string;
  amount: number;
  count: number;
}

export interface CashSession {
  id: string;
  attendant: string;
  openingAmount: number;
  expectedCashAmount: number; // Opening + cash sales only
  startAt: string; // ISO date string
  endAt?: string; // ISO date string
  closingAmount?: number;
  notes?: string;
  status: CashSessionStatus;
  salesTotals: {
    totalRevenue: number;
    totalOrders: number;
  };
  paymentBreakdown: PaymentMethodTotal[];
}

export const createCashSessionFormSchema = z.object({
  openingAmount: z
    .number({ error: "O valor de abertura deve ser um número" })
    .min(0, "O valor de abertura não pode ser negativo"),
  notes: z.string().optional(),
});

export type CreateCashSessionFormData = z.infer<
  typeof createCashSessionFormSchema
>;

export const closeCashSessionFormSchema = z.object({
  closingAmount: z
    .number({ error: "O valor de fechamento deve ser um número" })
    .min(0, "O valor de fechamento não pode ser negativo"),
  notes: z.string().optional(),
});

export type CloseCashSessionFormData = z.infer<
  typeof closeCashSessionFormSchema
>;
