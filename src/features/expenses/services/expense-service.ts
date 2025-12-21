import { type ApiResponse, api } from "@/shared/services/api";
import type { Category, Expense, ExpenseFormData } from "../types";

interface CategoryDto {
  id: string;
  name: string;
  isActive: boolean;
  subCategories: CategoryDto[];
}

interface ExpenseDto {
  id: string;
  category: CategoryDto;
  supplier?: string | null;
  description?: string | null;
  amount: number;
  dueDate: string;
  paymentDate?: string | null;
  status: string;
  createdAt: string;
  updatedAt: string;
}

function mapCategory(dto: CategoryDto): Category {
  return {
    id: dto.id,
    name: dto.name,
    isActive: dto.isActive,
    subCategories: dto.subCategories.map(mapCategory),
  };
}

function parseDateOnly(dateString?: string | null): Date | undefined {
  if (!dateString) return undefined;

  const isoOnly = /^(\d{4})-(\d{2})-(\d{2})$/.exec(dateString);
  if (isoOnly) {
    const [, y, m, d] = isoOnly;
    return new Date(Number(y), Number(m) - 1, Number(d));
  }

  const slashOnly = /^(\d{4})\/(\d{2})\/(\d{2})$/.exec(dateString);
  if (slashOnly) {
    const [, y, m, d] = slashOnly;
    return new Date(Number(y), Number(m) - 1, Number(d));
  }

  const dt = new Date(dateString);
  return Number.isNaN(dt.getTime()) ? undefined : dt;
}

function mapExpense(dto: ExpenseDto): Expense {
  return {
    id: dto.id,
    category: mapCategory(dto.category),
    supplier: dto.supplier ?? undefined,
    description: dto.description ?? undefined,
    amount: dto.amount,
    // biome-ignore lint/style/noNonNullAssertion: <explanation> We are sure the date strings are valid.</explanation>
    dueDate: parseDateOnly(dto.dueDate)!,
    paymentDate: parseDateOnly(dto.paymentDate),
    status: dto.status as Expense["status"],
    createdAt: new Date(dto.createdAt),
    updatedAt: new Date(dto.updatedAt),
  };
}

export const expenseService = {
  getAll: async (params?: {
    startDate?: string;
    endDate?: string;
    categoryId?: string;
    status?: string;
  }): Promise<Expense[]> => {
    let url = "/api/expenses";
    if (
      params?.startDate ||
      params?.endDate ||
      params?.categoryId ||
      params?.status
    ) {
      const qp = new URLSearchParams();
      if (params.startDate) qp.set("start", params.startDate);
      if (params.endDate) qp.set("end", params.endDate);
      if (params.categoryId) qp.set("categoryId", params.categoryId);
      if (params.status) qp.set("status", params.status);
      url = `${url}?${qp.toString()}`;
    }
    const res = await api.get<ApiResponse<ExpenseDto[] | null>>(url);
    const list = res.data ?? [];
    return list.map(mapExpense);
  },

  getById: async (id: string): Promise<Expense | null> => {
    const res = await api.get<ApiResponse<ExpenseDto | null>>(
      `/api/expenses/${id}`,
    );
    return res.data ? mapExpense(res.data) : null;
  },

  create: async (data: ExpenseFormData): Promise<void> => {
    const payload = {
      categoryId: data.subcategoryId || data.categoryId,
      amount: data.amount,
      dueDate: data.dueDate,
      supplier: data.supplier || null,
      description: data.description || null,
      paymentDate: data.paymentDate || null,
    };
    await api.post<void>("/api/expenses", payload);
  },

  update: async (id: string, data: Partial<ExpenseFormData>): Promise<void> => {
    const payload = {
      expenseId: id,
      categoryId: data.categoryId || undefined,
      amount: data.amount || undefined,
      dueDate: data.dueDate || undefined,
      supplier: data.supplier || null,
      description: data.description || null,
    };
    await api.put<void>(`/api/expenses/${id}`, payload);
  },

  markAsPaid: async (id: string, paymentDate: string): Promise<void> => {
    await api.patch<void>(`/api/expenses/${id}/mark-as-paid`, {
      expenseId: id,
      paymentDate,
    });
  },

  delete: async (id: string): Promise<void> => {
    await api.delete<void>(`/api/expenses/${id}`);
  },

  getAllCategories: async (): Promise<Category[]> => {
    const res = await api.get<ApiResponse<CategoryDto[] | null>>(
      "/api/expenses/categories",
    );
    const list = res.data ?? [];
    return list.map(mapCategory);
  },
};
