import { useQuery } from "@tanstack/react-query";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardSales(
  startDate?: Date,
  endDate?: Date,
  enabled = true,
) {
  const paymentBreakdownQuery = useQuery({
    queryKey: ["dashboard", "payment-breakdown", { startDate, endDate }],
    queryFn: () => dashboardService.getPaymentBreakdown(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const topProductsQuery = useQuery({
    queryKey: ["dashboard", "top-products", { startDate, endDate }],
    queryFn: () => dashboardService.getTopProducts(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const salesOverTimeQuery = useQuery({
    queryKey: ["dashboard", "sales-over-time", { startDate, endDate }],
    queryFn: () => dashboardService.getSalesOverTime(startDate, endDate),
    enabled,
    staleTime: 30_000,
    placeholderData: (previousData) => previousData,
  });

  const isFetching =
    paymentBreakdownQuery.isFetching ||
    topProductsQuery.isFetching ||
    salesOverTimeQuery.isFetching;

  return {
    paymentBreakdown: paymentBreakdownQuery.data ?? {
      breakdown: { Cash: 0, CreditCard: 0, DebitCard: 0, Pix: 0 },
      total: 0,
    },
    topProducts: topProductsQuery.data ?? [],
    salesOverTime: salesOverTimeQuery.data ?? [],
    isFetching,
  };
}
