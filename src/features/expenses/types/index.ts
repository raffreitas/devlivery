import { z } from "zod";

// Status das despesas
export const ExpenseStatus = {
  PAID: "Paid",
  PENDING: "Pending",
  OVERDUE: "Overdue",
  DUE_TODAY: "DueToday",
  CANCELLED: "Cancelled",
} as const;

export type ExpenseStatus = (typeof ExpenseStatus)[keyof typeof ExpenseStatus];

// Category - representa categoria e subcategoria
export interface Category {
  id: string;
  name: string;
  isActive: boolean;
  subCategories: Category[];
}

// Interface principal da despesa
export interface Expense {
  id: string;
  category: Category;
  supplier?: string;
  description?: string;
  amount: number;
  dueDate: Date;
  paymentDate?: Date;
  status: ExpenseStatus;
  createdAt: Date;
  updatedAt: Date;
}

// Schema Zod para validação do formulário
export const expenseFormSchema = z.object({
  categoryId: z.uuid("Categoria é obrigatória"),
  subcategoryId: z.string().optional(),
  supplier: z.string().optional(),
  description: z.string().optional(),
  amount: z
    .number({
      message: "Valor é obrigatório",
    })
    .positive("Valor deve ser maior que zero"),
  dueDate: z.string().min(1, "Data de vencimento é obrigatória"),
  paymentDate: z.string().optional(),
});

export type ExpenseFormData = z.infer<typeof expenseFormSchema>;

// Filtros
export interface ExpenseFilters {
  startDate?: Date;
  endDate?: Date;
  categoryId?: string;
  status?: ExpenseStatus;
}

// Sumário/Totalizadores
export interface ExpenseSummary {
  total: number;
  paid: number;
  pending: number;
  overdue: number;
  count: number;
}
