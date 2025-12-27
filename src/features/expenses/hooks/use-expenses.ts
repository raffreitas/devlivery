import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo } from "react";
import type { DateRange } from "react-day-picker";
import { formatDate } from "@/shared/utils/formatters";
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
        startDate: duePeriod?.from,
        endDate: duePeriod?.to,
        categoryId,
        status,
      },
    ],
    queryFn: () =>
      expenseService.getAll({
        startDate: duePeriod?.from ? formatDate(duePeriod.from) : undefined,
        endDate: duePeriod?.to ? formatDate(duePeriod.to) : undefined,
        categoryId,
        status,
      }),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  // Calcula sumário/totalizadores
  const summary: ExpenseSummary = useMemo(() => {
    const expenses = expensesQuery.data ?? [];

    return {
      total: expenses.reduce((sum, exp) => sum + exp.amount, 0),
      paid: expenses
        .filter((exp) => exp.status === "Paid")
        .reduce((sum, exp) => sum + exp.amount, 0),
      pending: expenses
        .filter((exp) => exp.status === "Pending" || exp.status === "DueToday")
        .reduce((sum, exp) => sum + exp.amount, 0),
      overdue: expenses
        .filter((exp) => exp.status === "Overdue")
        .reduce((sum, exp) => sum + exp.amount, 0),
      count: expenses.length,
    };
  }, [expensesQuery.data]);

  const createMutation = useMutation({
    mutationFn: expenseService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expenses"] });
      queryClient.invalidateQueries({ queryKey: ["cash-sessions"] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: Partial<ExpenseFormData>;
    }) => expenseService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expenses"] });
      queryClient.invalidateQueries({ queryKey: ["cash-sessions"] });
    },
  });

  const markAsPaidMutation = useMutation({
    mutationFn: ({ id, paymentDate }: { id: string; paymentDate: string }) =>
      expenseService.markAsPaid(id, paymentDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expenses"] });
      queryClient.invalidateQueries({ queryKey: ["cash-sessions"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => expenseService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expenses"] });
      queryClient.invalidateQueries({ queryKey: ["cash-sessions"] });
    },
  });

  return {
    expenses: expensesQuery.data ?? [],
    summary,
    loading: expensesQuery.isLoading,
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
    queryKey: ["expenses", id],
    queryFn: () => expenseService.getById(id as string),
    enabled: !!id,
    staleTime: 30_000,
  });
}

export function useExpenseCategories() {
  return useQuery({
    queryKey: ["expense-categories"],
    queryFn: () => expenseService.getAllCategories(),
    staleTime: 60_000, // Cache por 1 minuto (categorias não mudam frequentemente)
  });
}

export function useExpenseCategoriesManagement() {
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: { name: string; parentCategoryId?: string }) =>
      expenseService.createCategory(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expense-categories"] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: { name?: string; isActive?: boolean };
    }) => expenseService.updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expense-categories"] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => expenseService.deleteCategory(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["expense-categories"] });
    },
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
