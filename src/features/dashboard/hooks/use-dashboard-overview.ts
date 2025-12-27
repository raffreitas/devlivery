import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardOverview(startDate?: Date, endDate?: Date) {
  const statsQuery = useQuery({
    queryKey: ["dashboard", "stats", { startDate, endDate }],
    queryFn: () => dashboardService.getStats(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expenseSummaryQuery = useQuery({
    queryKey: ["dashboard", "expense-summary", { startDate, endDate }],
    queryFn: () => dashboardService.getExpenseSummary(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const salesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "sales-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getSalesOverTime(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "expenses-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesOverTime(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const upcomingExpensesQuery = useQuery({
    queryKey: ["dashboard", "upcoming-expenses"],
    queryFn: () => dashboardService.getUpcomingExpenses(7),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesByStatusQuery = useQuery({
    queryKey: ["dashboard", "expenses-by-status", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesByStatus(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const isFetching =
    statsQuery.isFetching ||
    expenseSummaryQuery.isFetching ||
    salesOverTimeQuery.isFetching ||
    expensesOverTimeQuery.isFetching ||
    upcomingExpensesQuery.isFetching ||
    expensesByStatusQuery.isFetching;

  return {
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
    upcomingExpenses: upcomingExpensesQuery.data ?? [],
    expensesByStatus: expensesByStatusQuery.data ?? [],
    isFetching,
  };
}
