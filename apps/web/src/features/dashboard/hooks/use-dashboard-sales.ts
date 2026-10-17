import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardSales(
  startDate?: Date,
  endDate?: Date,
  enabled = true,
) {
  const paymentBreakdownQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "payment-breakdown",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getPaymentBreakdown(startDate, endDate, signal),
    enabled,
    staleTime: 30_000,
  });

  const topProductsQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "top-products",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getTopProducts(startDate, endDate, signal),
    enabled,
    staleTime: 30_000,
  });

  const isFetching =
    paymentBreakdownQuery.isFetching || topProductsQuery.isFetching;

  return {
    queries: {
      paymentBreakdown: paymentBreakdownQuery,
      topProducts: topProductsQuery,
    },
    paymentBreakdown: paymentBreakdownQuery.data ?? {
      breakdown: { Cash: 0, CreditCard: 0, DebitCard: 0, Pix: 0 },
      total: 0,
    },
    topProducts: topProductsQuery.data ?? [],
    isFetching,
  };
}
