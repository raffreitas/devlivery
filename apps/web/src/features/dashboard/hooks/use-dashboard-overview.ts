import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardOverview(startDate?: Date, endDate?: Date) {
  const statsQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "stats",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getStats(startDate, endDate, signal),
    staleTime: 30_000,
  });

  const expenseSummaryQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "expense-summary",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getExpenseSummary(startDate, endDate, signal),
    staleTime: 30_000,
  });

  const salesOverTimeQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "sales-over-time",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getSalesOverTime(startDate, endDate, signal),
    staleTime: 30_000,
  });

  const expensesOverTimeQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "expenses-over-time",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getExpensesOverTime(startDate, endDate, signal),
    staleTime: 30_000,
  });

  const expensesByStatusQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "expenses-by-status",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getExpensesByStatus(startDate, endDate, signal),
    staleTime: 30_000,
  });

  const isFetching =
    statsQuery.isFetching ||
    expenseSummaryQuery.isFetching ||
    salesOverTimeQuery.isFetching ||
    expensesOverTimeQuery.isFetching ||
    expensesByStatusQuery.isFetching;

  return {
    queries: {
      stats: statsQuery,
      expenseSummary: expenseSummaryQuery,
      salesOverTime: salesOverTimeQuery,
      expensesOverTime: expensesOverTimeQuery,
      expensesByStatus: expensesByStatusQuery,
    },
    stats: statsQuery.data ?? {
      totalOrders: 0,
      totalRevenue: 0,
      pendingOrders: 0,
      deliveredOrders: 0,
      averageOrderValue: 0,
    },
    expenseSummary: expenseSummaryQuery.data ?? {
      total: 0,
      paid: 0,
      pending: 0,
      overdue: 0,
      count: 0,
    },
    salesOverTime: salesOverTimeQuery.data ?? [],
    expensesOverTime: expensesOverTimeQuery.data ?? [],
    expensesByStatus: expensesByStatusQuery.data ?? [],
    isFetching,
  };
}
