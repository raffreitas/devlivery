import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "@/shared/services/query-keys";
import { cashService } from "../services/cash-service";

const CASH_SESSIONS_QUERY_KEY = queryKeys.cash;

export function useCurrentCashSession() {
  const currentSessionQuery = useQuery({
    queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
    queryFn: ({ signal }) => cashService.getActive(signal),
    staleTime: 0,
    gcTime: 5 * 60 * 1000,
    placeholderData: (previousData) => previousData,
  });

  return {
    currentSession: currentSessionQuery.data ?? null,
    isLoading: currentSessionQuery.isLoading,
    isFetching: currentSessionQuery.isFetching,
    error: currentSessionQuery.error,
  };
}
