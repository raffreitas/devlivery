import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";
import { dashboardService } from "../services/dashboard-service";

export function useDashboardExpenses(
  startDate?: Date,
  endDate?: Date,
  enabled = true,
) {
  const expensesByCategoryQuery = useQuery({
    queryKey: [
      ...queryKeys.dashboard,
      "expenses-by-category",
      queryKeys.period(startDate, endDate),
    ],
    queryFn: ({ signal }) =>
      dashboardService.getExpensesByCategory(startDate, endDate, signal),
    enabled,
    staleTime: 30_000,
  });

  const isFetching = expensesByCategoryQuery.isFetching;

  return {
    queries: { expensesByCategory: expensesByCategoryQuery },
    expensesByCategory: expensesByCategoryQuery.data ?? [],
    isFetching,
  };
}
