import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo } from "react";
import type { DateRange } from "react-day-picker";
import {
  invalidateAfterMutation,
  queryKeys,
} from "@/shared/services/query-keys";
import { formatDate } from "@/shared/utils/formatters";
import { fromCents, toCents } from "@/shared/utils/money";
import { expenseService } from "../services/expense-service";
import type { ExpenseFormData, ExpenseStatus, ExpenseSummary } from "../types";

type ExpenseInput = {
  duePeriod?: DateRange;
  categoryId?: string;
  status?: ExpenseStatus;
};

export function useExpenses({ duePeriod, categoryId, status }: ExpenseInput) {
  const queryClient = useQueryClient();

  const expensesQuery = useQuery({
    queryKey: [
      "expenses",
      {
        ...queryKeys.period(duePeriod?.from, duePeriod?.to),
        categoryId,
        status,
      },
    ],
    queryFn: ({ signal }) =>
      expenseService.getAll(
        {
          startDate: duePeriod?.from ? formatDate(duePeriod.from) : undefined,
          endDate: duePeriod?.to ? formatDate(duePeriod.to) : undefined,
          categoryId,
          status,
        },
        signal,
      ),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const summary: ExpenseSummary = useMemo(() => {
    const expenses = expensesQuery.data ?? [];

    return {
      total: fromCents(
        expenses.reduce((sum, exp) => sum + toCents(exp.amount), 0),
      ),
      paid: fromCents(
        expenses
          .filter((exp) => exp.status === "Paid")
          .reduce((sum, exp) => sum + toCents(exp.amount), 0),
      ),
      pending: fromCents(
        expenses
          .filter(
            (exp) => exp.status === "Pending" || exp.status === "DueToday",
          )
          .reduce((sum, exp) => sum + toCents(exp.amount), 0),
      ),
      overdue: fromCents(
        expenses
          .filter((exp) => exp.status === "Overdue")
          .reduce((sum, exp) => sum + toCents(exp.amount), 0),
      ),
      count: expenses.length,
    };
  }, [expensesQuery.data]);

  const createMutation = useMutation({
    mutationFn: expenseService.create,
    meta: { skipGlobalErrorToast: true },
    onSuccess: () => invalidateAfterMutation(queryClient, "expenses"),
  });

  const updateMutation = useMutation({
    meta: { skipGlobalErrorToast: true },
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Partial<ExpenseFormData>;
    }) => expenseService.update(id, data),
    onSuccess: () => invalidateAfterMutation(queryClient, "expenses"),
  });

  const markAsPaidMutation = useMutation({
    mutationFn: ({ id, paymentDate }: { id: string; paymentDate: string }) =>
      expenseService.markAsPaid(id, paymentDate),
    onSuccess: () => invalidateAfterMutation(queryClient, "expenses"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => expenseService.delete(id),
    onSuccess: () => invalidateAfterMutation(queryClient, "expenses"),
  });

  return {
    expenses: expensesQuery.data ?? [],
    summary,
    isSaving: createMutation.isPending || updateMutation.isPending,
    loading: expensesQuery.isLoading,
    error: expensesQuery.error,
    hasData: expensesQuery.data !== undefined,
    isFetching: expensesQuery.isFetching,
    refetch: expensesQuery.refetch,
    createExpense: (data: ExpenseFormData) => createMutation.mutateAsync(data),
    updateExpense: (id: string, data: Partial<ExpenseFormData>) =>
      updateMutation.mutateAsync({ id, data }),
    markAsPaid: (id: string, paymentDate: string) =>
      markAsPaidMutation.mutateAsync({ id, paymentDate }),
    deleteExpense: (id: string) => deleteMutation.mutateAsync(id),
  };
}

export function useExpense(id: string | undefined) {
  return useQuery({
    queryKey: [...queryKeys.expenses, id],
    queryFn: ({ signal }) => expenseService.getById(id as string, signal),
    enabled: !!id,
    staleTime: 30_000,
  });
}

export function useExpenseCategories() {
  return useQuery({
    queryKey: queryKeys.categories,
    queryFn: ({ signal }) => expenseService.getAllCategories(signal),
    staleTime: 60_000, // Cache por 1 minuto (categorias não mudam frequentemente)
  });
}

export function useExpenseCategoriesManagement(
  options: { skipGlobalErrorToast?: boolean } = {},
) {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    meta: options,
    mutationFn: (data: { name: string; parentCategoryId?: string }) =>
      expenseService.createCategory(data),
    onSuccess: () => invalidateAfterMutation(queryClient, "categories"),
  });

  const updateMutation = useMutation({
    meta: options,
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: { name?: string; isActive?: boolean };
    }) => expenseService.updateCategory(id, data),
    onSuccess: () => invalidateAfterMutation(queryClient, "categories"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => expenseService.deleteCategory(id),
    onSuccess: () => invalidateAfterMutation(queryClient, "categories"),
  });

  return {
    createCategory: (data: { name: string; parentCategoryId?: string }) =>
      createMutation.mutateAsync(data),
    updateCategory: (id: string, data: { name?: string; isActive?: boolean }) =>
      updateMutation.mutateAsync({ id, data }),
    deleteCategory: (id: string) => deleteMutation.mutateAsync(id),
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
