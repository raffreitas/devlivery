import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardExpenses(
  startDate?: Date,
  endDate?: Date,
  enabled = true,
) {
  const expensesByCategoryQuery = useQuery({
    queryKey: ["dashboard", "expenses-by-category", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesByCategory(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesByStatusQuery = useQuery({
    queryKey: ["dashboard", "expenses-by-status", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesByStatus(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "expenses-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesOverTime(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const isFetching =
    expensesByCategoryQuery.isFetching ||
    expensesByStatusQuery.isFetching ||
    expensesOverTimeQuery.isFetching;

  return {
    expensesByCategory: expensesByCategoryQuery.data ?? [],
    expensesByStatus: expensesByStatusQuery.data ?? [],
    expensesOverTime: expensesOverTimeQuery.data ?? [],
    isFetching,
  };
}

