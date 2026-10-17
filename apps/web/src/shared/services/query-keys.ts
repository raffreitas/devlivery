import type { QueryClient } from "@tanstack/react-query";
import { formatDate } from "@/shared/utils/formatters";

export const queryKeys = {
  products: ["products"] as const,
  orders: ["orders"] as const,
  expenses: ["expenses"] as const,
  categories: ["expense-categories"] as const,
  dashboard: ["dashboard"] as const,
  cash: ["cash-sessions"] as const,
  currentCash: ["cash-sessions", "current"] as const,
  deposits: (id?: string) => ["cash-sessions", "deposits", id] as const,
  period: (startDate?: Date, endDate?: Date) => ({
    startDate: startDate ? formatDate(startDate) : undefined,
    endDate: endDate ? formatDate(endDate) : undefined,
  }),
};

const affectedQueries = {
  orders: [queryKeys.orders, queryKeys.dashboard, queryKeys.cash],
  expenses: [queryKeys.expenses, queryKeys.dashboard, queryKeys.cash],
  products: [queryKeys.products, queryKeys.orders, queryKeys.dashboard],
  categories: [queryKeys.categories, queryKeys.expenses, queryKeys.dashboard],
  cash: [queryKeys.cash],
} as const;

export async function invalidateAfterMutation(
  client: QueryClient,
  feature: keyof typeof affectedQueries,
) {
  // A successful write stays successful even if a subsequent refresh fails.
  await Promise.allSettled(
    affectedQueries[feature].map((queryKey) =>
      client.invalidateQueries({ queryKey }),
    ),
  );
}
