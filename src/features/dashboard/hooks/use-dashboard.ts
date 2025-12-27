import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "../services/dashboard-service";

/**
 * @deprecated Este hook foi substituído por hooks específicos por tab:
 * - useDashboardOverview: dados sempre necessários
 * - useDashboardSales: dados da tab Vendas
 * - useDashboardExpenses: dados da tab Despesas
 *
 * Use os hooks específicos para melhor performance e carregamento sob demanda.
 */
export function useDashboard(startDate?: Date, endDate?: Date) {
  const statsQuery = useQuery({
    queryKey: ["dashboard", "stats", { startDate, endDate }],
    queryFn: () => dashboardService.getStats(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const paymentBreakdownQuery = useQuery({
    queryKey: ["dashboard", "payment-breakdown", { startDate, endDate }],
    queryFn: () => dashboardService.getPaymentBreakdown(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const ordersByStatusQuery = useQuery({
    queryKey: ["dashboard", "orders-by-status", { startDate, endDate }],
    queryFn: () => dashboardService.getOrdersByStatus(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const salesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "sales-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getSalesOverTime(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const topProductsQuery = useQuery({
    queryKey: ["dashboard", "top-products", { startDate, endDate }],
    queryFn: () => dashboardService.getTopProducts(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesByCategoryQuery = useQuery({
    queryKey: ["dashboard", "expenses-by-category", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesByCategory(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesByStatusQuery = useQuery({
    queryKey: ["dashboard", "expenses-by-status", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesByStatus(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expensesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "expenses-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getExpensesOverTime(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const expenseSummaryQuery = useQuery({
    queryKey: ["dashboard", "expense-summary", { startDate, endDate }],
    queryFn: () => dashboardService.getExpenseSummary(startDate, endDate),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const upcomingExpensesQuery = useQuery({
    queryKey: ["dashboard", "upcoming-expenses"],
    queryFn: () => dashboardService.getUpcomingExpenses(7),
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const isFetching =
    statsQuery.isFetching ||
    paymentBreakdownQuery.isFetching ||
    ordersByStatusQuery.isFetching ||
    salesOverTimeQuery.isFetching ||
    topProductsQuery.isFetching ||
    expensesByCategoryQuery.isFetching ||
    expensesByStatusQuery.isFetching ||
    expensesOverTimeQuery.isFetching ||
    expenseSummaryQuery.isFetching ||
    upcomingExpensesQuery.isFetching;

  return {
    stats: statsQuery.data ?? {
      totalOrders: 0,
      totalRevenue: 0,
      pendingOrders: 0,
      deliveredOrders: 0,
      averageOrderValue: 0,
    },
    paymentBreakdown: paymentBreakdownQuery.data ?? {
      breakdown: { Cash: 0, CreditCard: 0, DebitCard: 0, Pix: 0 },
      total: 0,
    },
    ordersByStatus: ordersByStatusQuery.data ?? {
      Pending: 0,
      Preparing: 0,
      Ready: 0,
      Delivered: 0,
      Canceled: 0,
    },
    salesOverTime: salesOverTimeQuery.data ?? [],
    topProducts: topProductsQuery.data ?? [],
    expensesByCategory: expensesByCategoryQuery.data ?? [],
    expensesByStatus: expensesByStatusQuery.data ?? [],
    expensesOverTime: expensesOverTimeQuery.data ?? [],
    expenseSummary: expenseSummaryQuery.data ?? {
      total: 0,
      paid: 0,
      pending: 0,
      overdue: 0,
      count: 0,
    },
    upcomingExpenses: upcomingExpensesQuery.data ?? [],
    isFetching,
  };
}
