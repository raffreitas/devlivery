import { useQuery } from "@tanstack/react-query";
import { cashService } from "../services/cash-service";

const CASH_SESSIONS_QUERY_KEY = ["cash-sessions"];

/**
 * Hook to fetch only the current active cash session
 * More efficient than useCashSessions when you only need current session
 */
export function useCurrentCashSession() {
  const currentSessionQuery = useQuery({
    queryKey: [...CASH_SESSIONS_QUERY_KEY, "current"],
    queryFn: () => cashService.getActive(),
    staleTime: 0,
    placeholderData: (previousData) => previousData,
  });

  return {
    currentSession: currentSessionQuery.data ?? null,
    isLoading: currentSessionQuery.isLoading,
    isFetching: currentSessionQuery.isFetching,
    error: currentSessionQuery.error,
  };
}
